# Spec Delta

## Purpose

Define reliable template parsing that preserves literal content and documented escapes while rejecting incomplete expressions with actionable source positions.

## ADDED Requirements

### Requirement: Preserve literal content

The parser SHALL preserve literal text, including whitespace-only input, without trimming or normalizing line endings. Null and empty input SHALL remain valid empty templates.

#### Scenario: Whitespace-only template
- **WHEN** the template content is `" \n\t"`
- **THEN** rendering returns exactly `" \n\t"`

#### Scenario: Null or empty template
- **WHEN** null or an empty string is parsed and rendered
- **THEN** rendering returns an empty string

#### Scenario: Literal line endings
- **WHEN** `"a\r\nb\nc\t "` is parsed and rendered
- **THEN** the output exactly matches the input

### Requirement: Preserve documented delimiter escapes

The parser SHALL interpret `{{` as a literal opening brace, not as an expression opener. Literal closing braces outside expressions SHALL remain unchanged.

#### Scenario: Escaped tag
- **WHEN** `<h1>{{title}</h1>` is parsed and rendered
- **THEN** the output is `<h1>{title}</h1>` without resolving `title`

#### Scenario: Escaped tag with two closing braces
- **WHEN** `<h1>{{title}}</h1>` is parsed and rendered
- **THEN** the output is `<h1>{title}}</h1>`

#### Scenario: Literal closing braces
- **WHEN** `text}}` is parsed and rendered
- **THEN** the output is `text}}`

### Requirement: Require complete expressions

Every unescaped tag, loop selector, and nested expression SHALL consume its corresponding closing delimiter. Reaching end-of-input first SHALL throw a `FormatException` during parsing rather than producing a partial template. Escaped opening braces SHALL NOT introduce a delimiter-balance requirement.

#### Scenario: Unterminated ordinary tag
- **WHEN** `{title` is parsed
- **THEN** parsing throws a `FormatException` identifying the missing `}`

#### Scenario: Unterminated loop selector
- **WHEN** `{(rows` is parsed
- **THEN** parsing throws a `FormatException` identifying the missing `)`

#### Scenario: Unterminated loop body
- **WHEN** either `{(rows)` or `{(rows){item.Name}` is parsed
- **THEN** parsing throws a `FormatException` identifying the missing loop-body `}`

#### Scenario: Unterminated grouped expression
- **WHEN** `{ text` is parsed
- **THEN** parsing throws a `FormatException` identifying the missing `}`

#### Scenario: Complete loop and grouped expression
- **WHEN** `{(rows){ {item.Name}}}` is parsed
- **THEN** parsing succeeds with the complete expression structure

#### Scenario: Escaped opening brace at end-of-input
- **WHEN** `{{` is parsed and rendered
- **THEN** parsing succeeds and the output is `{`

### Requirement: Actionable parser diagnostics

Syntax failures SHALL report the expected delimiter or invalid selector character and a one-based line and column. End-of-input errors SHALL report the cursor position immediately after the last character. LF SHALL advance to the next line, CRLF SHALL count as one line break, and a tab SHALL count as one column. Parser-generated `FormatException` errors SHALL preserve their originating stack frames.

#### Scenario: End-of-input position after a newline
- **WHEN** `"x\n{title"` is parsed
- **THEN** the error identifies missing `}` at line 2, column 7

#### Scenario: CRLF source position
- **WHEN** `"x\r\n{title"` is parsed
- **THEN** the error identifies missing `}` at line 2, column 7

#### Scenario: Invalid selector character
- **WHEN** `"x\n{(rows!)}"` is parsed
- **THEN** parsing throws a `FormatException` identifying the invalid character at line 2, column 7

#### Scenario: Position following a complete tag
- **WHEN** `"{title}{broken"` is parsed
- **THEN** the error identifies missing `}` at line 1, column 15

#### Scenario: Parser exception stack preservation
- **WHEN** a node parser raises a syntax `FormatException`
- **THEN** the propagated exception retains that node parser's originating stack frame
