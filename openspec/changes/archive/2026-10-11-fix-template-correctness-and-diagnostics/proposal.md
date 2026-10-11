# Proposal

## Why

The source review reproduced incorrect nested member resolution, inconsistent culture formatting, acceptance of incomplete expressions, lost loop context, and discarded literal whitespace. Silent member-access failures and a single legacy-runtime happy-path test make these defects difficult to detect and diagnose.

## What Changes

- Resolve multi-segment item property and field paths against each successive value.
- Format resolver-provided scalar values with the render's effective culture, consistently with item values.
- Preserve item, index, and order when rendering non-loop nested expressions.
- Preserve whitespace-only templates while retaining existing null and empty input behavior.
- **BREAKING**: Reject unterminated tags, loop selectors, and nested expressions with position-aware `FormatException` errors instead of accepting incomplete templates.
- **BREAKING**: Report missing item members as descriptive errors. Render null items, intermediate values, and leaf values as empty text; surface getter failures with their original exception and member-path context.
- Preserve parser exception stacks and restrict exception wrapping to meaningful diagnostics.
- Add regression and compatibility coverage; replace `async void` tests with `async Task`.
- Move the test project to .NET 10 LTS and update its existing test dependencies as needed, without changing the library's `netstandard2.0` target.
- Document grouped expressions, member/null/error behavior, culture formatting, test commands, and migration from previously tolerated invalid input.

## Capabilities

### New Capabilities

- `template-parsing`: Literal preservation, existing escape syntax, complete expression validation, and accurate parser diagnostics.
- `template-rendering`: Nested member traversal, culture-aware rendering, scoped loop context, and explicit rendering failures.

These describe existing engine surfaces for the first time; the project currently has no main specifications.

### Modified Capabilities

None.

## Impact

- Implementation: `TemplateEngine.cs`, `TagNode.cs`, `NestedTagNode.cs`, `NestedExpressionNode.cs`, and `TextNode.cs`.
- Tests: `UnitTest1.cs`, test fixtures/resolvers, and `Kkts.Template.UnitTest.csproj`.
- Documentation: `README.md`.
- Public method signatures, resolver interface, library target framework, and valid documented template syntax remain unchanged.
- Consumers relying on truncated templates or silent missing-member output must correct their templates/data before adopting the change. A future release must communicate these breaking behaviors; package publication and version assignment are outside this proposal.
- No new production dependencies, HTML escaping, new loop-selector semantics, render-cache redesign, or general performance framework.
