# Design

## Context

See [proposal.md](./proposal.md) for motivation and compatibility impact. The engine is a small, dependency-free `netstandard2.0` library with character-based node readers. Index, line, and column are passed by reference across readers; inconsistent delimiter advancement makes diagnostics fragile.

`TemplateEngine.Resolve` builds an expression parameter for the original item once, then retrieves every path segment from that same item. Its broad catch hides both lookup and getter errors. Recursive rendering supplies a fresh item/index for loops but incorrectly does so for non-loop groups too. Resolver values use `StringBuilder.Append(object)`, whereas item values already use culture-aware conversion.

The repository has no existing main specs. The test project uses xUnit v2, targets `netcoreapp3.1`, and contains one `async void` integration test. The review ran that test successfully only with ARM64 test-host configuration and runtime roll-forward; isolated probes on .NET 10 reproduced the defects.

## Goals / Non-Goals

**Goals:**
- Make the smallest coherent changes to node readers and rendering while preserving valid syntax, public signatures, and library runtime compatibility.
- Turn each observed defect into a deterministic regression test and expose useful diagnostic context.
- Run tests normally on the supported .NET 10 LTS runtime, without architecture overrides or runtime roll-forward.

**Non-Goals:**
- A parser rewrite, regex-based grammar, new production packages, compiled accessor cache, or render-context object hierarchy.
- New expression operators, parent-loop references, item-relative loop selectors, HTML encoding, cache-lifetime changes, or package publication.
- Tightening identifier grammar beyond existing rules except where needed to detect incomplete delimiters.

## Decisions

### 1. Traverse using the current receiver

Maintain a current value initialized to the data item. For each path segment, resolve the member on the current value's runtime type and read it from that value; stop with empty text when the current value becomes null. Reuse the existing property/field lookup semantics where practical, constructing any lookup expression for the current receiver type rather than the original type. Characterize inherited members and case matching before replacing that lookup mechanism.

This fixes the root cause without introducing caching or compiled expressions. Alternatives: compiling full accessors adds unnecessary complexity; only updating the final read receiver still looks up members on the wrong type.

### 2. Explicit member-access failures

Follow the user's selected policy: nulls render empty; missing members on non-null receivers fail. Use `InvalidOperationException` for missing members and getter failures, including the full `item.*` path, failing segment, and receiver type. Catch only the expected lookup failure and reflection getter wrapper where context is added. For `TargetInvocationException`, use its original getter exception as the wrapper's inner exception. Do not catch all rendering exceptions or replace resolver failures with empty output.

No new public exception type or configurable leniency mode is needed. Returning empty for missing members was rejected because it preserves the diagnostic defect. Avoid logging inside the library; faulting the render task explicitly lets callers report failures using their own logging.

### 3. One formatting contract and existing recursion parameters

Use `Convert.ToString(value, cultureInfo)` for resolver scalars, matching whole-item and member conversion. Keep the existing cache and async resolver flow unchanged. For non-loop nodes, forward the current index and item; for loops, keep the existing locally scoped iteration variables. No context abstraction is necessary.

Alternatives: changing ambient culture is unsafe for callers; formatting inside the resolver would leave engine behavior inconsistent; merging caches across recursion would alter unrelated resolver-call behavior.

### 4. Validate closure with explicit cursor ownership

Retain node-based character parsing. Each reader must distinguish encountering its matching delimiter from exhausting input; tag readers validate `}`, selector readers validate `)`, and nested-expression readers validate their body `}`. Escaped `{{` is literal and must not create an unmatched opener. A parent must not count a child's consumed delimiter as its own closure.

Document a consistent ownership rule for index and position advancement across `ParseNodes`, `TagNode`, `NestedTagNode`, `NestedExpressionNode`, and `TextNode`. Count every consumed character exactly once, including delimiters, with LF starting the next line at column 1 and tabs taking one column. CRLF keeps its literal bytes and increments the line only at LF. Preserve existing ref-based interfaces; extract a small shared advancement/error helper only if needed to avoid duplicated logic.

Report invalid selectors at the offending character, and incomplete expressions at the end-of-input cursor with the missing delimiter. Replace `throw ex;` with `throw;` or remove that redundant catch. Replace broad parser exception wrapping with explicit parser validation and narrowly justified wrapping; unexpected implementation failures must not be mislabeled as syntax errors.

A global brace-counting pre-pass was rejected because escape and nested-node consumption make raw brace counts inaccurate. A full parser rewrite is unnecessary and risks documented literal behavior.

### 5. Preserve literal input

Only null or empty input returns an empty node list. Whitespace-only strings pass through the existing text reader, preserving bytes. This keeps the historical null contract without silently deleting intentional formatting.

### 6. Modernize tests without raising the library floor

Retarget only `Kkts.Template.UnitTest.csproj` to `net10.0`; keep `Kkts.Template.csproj` at `netstandard2.0` and retain its public signatures. Stay with xUnit v2 and VSTest, updating the existing SDK, adapter, and coverage packages to compatible stable versions as needed. Do not introduce Microsoft.Testing.Platform, switch test frameworks, or upgrade unrelated dependencies.

Rename the generic test class/file to a descriptive integration-test name if appropriate; retain its original input/output fixture. Add focused parser and renderer tests with small local resolvers, nested property/field fixtures, and a throwing getter. Use `Task` for async tests. Culture tests must restore prior culture in `finally` and avoid changing process-wide defaults.

Validate individual regression groups first, then the complete small test suite and library build. Tests must run without `DOTNET_ROLL_FORWARD` or `RunConfiguration.TargetPlatform` workarounds. Dependency version selection is an implementation-time mechanical check of compatible stable releases, not a change to the chosen framework or runtime.

### 7. Document intentionally changed behavior

Update `README.md` with member traversal, grouped expressions, culture examples, and a missing-member/null/getter behavior table. Add migration notes for unterminated expressions and previously hidden missing members, plus the .NET 10 test prerequisites and normal command. Keep existing escape examples and explain that literal closing braces outside expressions remain unchanged.

## Risks / Trade-offs

- [Consumers relied on incomplete templates or empty missing-member output] -> Mark both changes as breaking and provide concrete migration examples before release.
- [Delimiter bookkeeping causes skipped characters or premature closures] -> Test adjacent tags, nested loops, groups, escaped braces, EOF immediately after delimiters, and multiline diagnostics against exact output/positions.
- [Member matching changes unintentionally] -> Characterize existing single-segment, inherited property/field, and casing behavior; preserve it while changing receiver traversal.
- [New formatting changes localized output] -> Assert explicit and default culture behavior for resolver, item, and member values while preserving strings.
- [Reflection wrapping obscures original errors] -> Assert original getter exception identity through `InnerException`.
- [Legacy test adapters do not support .NET 10 normally] -> Update the existing test stack, verify standard ARM64 execution, and preserve xUnit v2/VSTest.

## Migration Plan

1. Add tests and modernize only the test runtime/tooling.
2. Implement the focused rendering, parser, and diagnostic changes.
3. Run the regression suite, original integration fixture, full test project, and `netstandard2.0` build.
4. Document compatibility changes and review before release; leave the current package version unchanged in this implementation.
5. Release owners select an appropriately communicated breaking release. Consumers correct missing data members and close incomplete expressions before upgrading.
6. Rollback, if a published change causes incompatibility, is to pin the previous package release while correcting templates/data. No persisted data migration is required.
