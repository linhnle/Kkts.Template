# Spec Delta

## Purpose

Define consistent template rendering across item paths, requested cultures, and nested loop contexts, with explicit errors instead of concealed member-access failures.

## ADDED Requirements

### Requirement: Traverse nested item members

Item paths SHALL resolve each property or field against the preceding segment's value. Existing single-segment property/field access and case-insensitive reserved keywords SHALL remain supported. Member-name matching SHALL preserve the engine's existing lookup behavior.

#### Scenario: Nested properties
- **WHEN** a loop item has `Address.City = "Hanoi"` and `{(rows){item.Address.City}}` is rendered
- **THEN** the output is `Hanoi`

#### Scenario: Mixed property and field path
- **WHEN** a loop item exposes an `Address` field whose value exposes a `City` property containing `Hanoi`
- **THEN** `{(rows){item.Address.City}}` renders `Hanoi`

#### Scenario: Single-segment compatibility
- **WHEN** an item exposes `Name = "Alice"` and `Id = 1`
- **THEN** `{(rows){item.Id}:{item.Name}}` renders `1:Alice`

### Requirement: Distinguish nulls from member failures

Null items, intermediate values, and leaf values SHALL render empty text. A missing member on a non-null object SHALL throw a descriptive `InvalidOperationException` identifying the full item path, failing segment, and receiver type. A failing getter SHALL throw an `InvalidOperationException` identifying the path and segment with the getter's original exception as its inner exception. Rendering SHALL NOT return a partial successful string on these failures.

#### Scenario: Null item
- **WHEN** a loop contains one null item
- **THEN** `{(rows)[{item.Name}]}` renders `[]`

#### Scenario: Null intermediate or leaf value
- **WHEN** an item has either a null `Address` or an `Address` with null `City`
- **THEN** `{(rows)[{item.Address.City}]}` renders `[]`

#### Scenario: Missing nested member
- **WHEN** a non-null address lacks a `City` member
- **THEN** rendering `{item.Address.City}` in that item's loop throws an error identifying `item.Address.City`, `City`, and the address type

#### Scenario: Throwing getter
- **WHEN** an item's `Name` getter throws a known exception
- **THEN** rendering its name faults with path/segment context and retains that exact exception as the inner exception

#### Scenario: Resolver failure
- **WHEN** the external tag resolver faults
- **THEN** rendering propagates the failure rather than substituting empty text

### Requirement: Consistent effective-culture formatting

Rendering SHALL use the explicitly supplied culture, or the current culture when none is supplied, for resolver values, whole items, and resolved item members. The same effective culture SHALL be passed to the resolver and nested renders. Null resolver values SHALL remain empty.

#### Scenario: Explicit culture differs from ambient culture
- **WHEN** the current culture is `en-US`, the requested culture is `fr-FR`, and `{price}` resolves to decimal `1.5`
- **THEN** the output is `1,5` and the resolver receives `fr-FR`

#### Scenario: Item and member formatting consistency
- **WHEN** `fr-FR` is requested and whole-item and item-member tags each resolve to decimal `1.5`
- **THEN** each renders `1,5`

#### Scenario: Default culture
- **WHEN** no culture is supplied, the current culture is `fr-FR`, and a value is decimal `1.5`
- **THEN** its output is `1,5` and the resolver receives `fr-FR`

#### Scenario: String and null resolver values
- **WHEN** two resolver tags return `"1.5"` and null respectively
- **THEN** rendering with `fr-FR` preserves `"1.5"` and renders the null value as empty text

### Requirement: Preserve context in grouped expressions

A non-loop nested expression SHALL inherit its enclosing item and iteration index. A nested loop SHALL establish its own item and index only for its body; subsequent sibling expressions SHALL retain the enclosing context.

#### Scenario: Grouped item
- **WHEN** the only row has `Name = "Alice"`
- **THEN** `{(rows){ {item.Name}}}` renders ` Alice`

#### Scenario: Grouped iteration metadata
- **WHEN** rows have names `Alice` and `Bob`
- **THEN** `{(rows){ [{index}:{order}:{item.Name}]}}` renders ` [0:1:Alice] [1:2:Bob]`

#### Scenario: Restore outer context after nested loop
- **WHEN** rows are `Alice` and `Bob` and an inner collection contains one item `X`
- **THEN** `{(rows){(inner){item}}:{item.Name}:{index};}` renders `X:Alice:0;X:Bob:1;`

### Requirement: Preserve existing documented rendering behavior

Valid documented templates SHALL retain literal text, tag replacement, opening-brace escapes, iteration ordering, and zero-based `index` and one-based `order`. Existing resolver-cache scope and top-level reserved-keyword behavior SHALL remain unchanged.

#### Scenario: Existing integration fixture
- **WHEN** the original SMS/email-style HTML fixture is rendered with its original resolver
- **THEN** the output exactly equals the original expected result

#### Scenario: Collection ordering
- **WHEN** `countries` resolves to `Vietnam` followed by `Japan`
- **THEN** `{(countries)<li>{order}:{item}</li>}` renders `<li>1:Vietnam</li><li>2:Japan</li>`

#### Scenario: Empty collection
- **WHEN** a loop resolver returns an empty collection
- **THEN** its body contributes no output
