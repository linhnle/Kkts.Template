# Tasks

## 1. Supported test harness and compatibility baseline

- [x] 1.1 Retarget `src/Kkts.Template.UnitTest/Kkts.Template.UnitTest.csproj` to `net10.0` and update existing test SDK, xUnit v2 adapter/framework, and coverage packages only as needed for compatibility; verify restore and ordinary `dotnet test src/Kkts.Template.UnitTest/Kkts.Template.UnitTest.csproj` discover and pass the original test without runtime roll-forward or architecture overrides.
- [x] 1.2 Change the original integration test to `async Task` and retain its exact fixture/output; add characterization tests for inherited properties/fields, member-name casing, reserved keywords, cache scope, and top-level behavior; verify the original fixture and these compatibility tests pass before changing rendering.
- [x] 1.3 Add .NET 10 SDK prerequisites and the standard test command to `README.md`; verify the documented command runs as written and the library project still targets `netstandard2.0`.

## 2. Nested member traversal and explicit errors

- [x] 2.1 Add focused renderer tests for nested properties and mixed property/field paths, including the `Hanoi` regression, and update member traversal to use each successive non-null receiver; verify targeted renderer tests and single-segment characterization tests pass.
- [x] 2.2 Implement empty rendering for null items/intermediate/leaf values and descriptive `InvalidOperationException` errors for missing members and getter failures; add tests asserting path, segment, receiver type, original getter exception identity, resolver-failure propagation, and no successful partial result; verify all targeted error/null tests pass.
- [x] 2.3 Document nested paths and the missing-member/null/getter policy in `README.md`, including migration from silently empty missing members; verify every documented success/error example has a matching passing regression test.

## 3. Culture formatting and grouped-expression context

- [x] 3.1 Apply the effective culture to resolver-value conversion, preserving string/null output; add explicit/default culture tests for resolver values, whole items, and members, assert the resolver's culture argument, and restore culture after each test; verify `1.5m` renders `1,5` under `fr-FR` with an `en-US` ambient culture.
- [x] 3.2 Preserve current item/index in non-loop nested expressions while retaining nested-loop scoping; add the ` Alice` grouped-item regression, two-row index/order coverage, and nested-loop sibling restoration tests; verify exact outputs from the rendering specification.
- [x] 3.3 Add culture and grouped-expression examples to `README.md` without introducing parent-loop or item-relative selector semantics; verify each example matches a passing test and existing integration output remains unchanged.

## 4. Literal preservation and parser correctness

- [x] 4.1 Preserve whitespace-only input while keeping null/empty behavior; add exact-output tests for spaces, tabs, LF, and CRLF; verify the parsing specification's literal-content scenarios pass.
- [x] 4.2 Add explicit closure validation to ordinary tags, loop selectors, and nested/grouped expressions, integrating index ownership across the existing node readers; add tests for all specified truncated inputs, adjacent tags, nested loops/groups, escaped braces, and EOF after closing delimiters; verify incomplete inputs throw during parsing and complete/escaped templates retain exact outputs.
- [x] 4.3 Correct line/column advancement, include the missing delimiter or invalid selector character in syntax errors, preserve original `FormatException` stacks, and remove broad unexpected-error-to-syntax conversion; add exact-position and stack-preservation tests; verify LF/CRLF EOF cases report line 2, column 7, the invalid selector case reports line 2, column 7, and the adjacent-tag case reports line 1, column 15.
- [x] 4.4 Document whitespace preservation, complete-delimiter requirements, diagnostic position conventions, and migration examples in `README.md`; retain the existing escape examples; verify all documented outputs/errors match the corresponding passing parser tests.

## 5. Cross-cutting verification

- [x] 5.1 Run the complete test project with `dotnet test src/Kkts.Template.UnitTest/Kkts.Template.UnitTest.csproj` and build the library with `dotnet build src/Kkts.Template/Kkts.Template.csproj`; verify all original/regression tests pass, `netstandard2.0` remains the library target, and the CA2200 rethrow warning is gone.
- [x] 5.2 Check every scenario in both delta specs against a passing test, verify README migration guidance and unchanged public signatures/production dependencies/package version, and run `openspec validate fix-template-correctness-and-diagnostics --strict`; completion requires successful validation and no unaddressed acceptance scenarios.
