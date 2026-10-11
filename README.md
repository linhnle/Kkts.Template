# Kkts.Template

A lightweight .NET template engine for SMS, emails, and formatted text, with asynchronous tag resolution, collection loops, and culture-aware formatting.

Kkts.Template parses templates without regular expressions. Your application supplies the values through `ITagResolver`; the engine handles substitution and iteration.

## Features

- Simple `{tag}` placeholders with an asynchronous resolver.
- Collection loops with item access and zero- or one-based positions.
- Nested property and field paths, such as `{item.Address.City}`.
- Culture-aware formatting for numbers, dates, and other resolver values.
- Literal whitespace preservation and opening-brace escaping.
- Syntax diagnostics with line and column positions.
- Targets .NET Standard 2.0 and is MIT licensed.

## Installation

Install [Kkts.Template from NuGet](https://www.nuget.org/packages/Kkts.Template/) in your application project:

```sh
dotnet add package Kkts.Template
```

The library targets `netstandard2.0`; your application must use a compatible runtime. The .NET 10 SDK is required only for this repository's test project.

## Quick start

The following example can be used in a modern C# console application's `Program.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Kkts.Template;

var engine = new TemplateEngine(new TagResolver());
var template = TemplateEngine.Parse("Hello {name}! Your order is {status}.");
var result = await engine.ResolveAsync(template);

Console.WriteLine(result);
// Hello Linh! Your order is ready.

sealed class TagResolver : ITagResolver
{
    private readonly Dictionary<string, object> values =
        new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = "Linh",
            ["status"] = "ready",
            ["price"] = 1.5m,
            ["countries"] = new[] { "Vietnam", "Japan" },
            ["cities"] = new[]
            {
                new { Id = 1, Name = "HCM" },
                new { Id = 2, Name = "Hanoi" }
            }
        };

    public Task<object> ResolveAsync(string tagName, CultureInfo cultureInfo)
    {
        if (!values.TryGetValue(tagName, out var value))
        {
            throw new InvalidOperationException($"Unknown tag: {tagName}");
        }

        return Task.FromResult(value);
    }
}
```

`TemplateEngine.Parse` creates a template that you can reuse in subsequent `ResolveAsync` calls. Implement `ITagResolver` to retrieve values from your application's data or services.

Unknown-tag behavior and tag-name case matching are controlled by your resolver. This example uses case-insensitive lookup and throws for unknown tags; returning `null` instead renders empty text. The engine caches resolver values by exact tag name within each render scope, not across render calls.

## Template syntax

These examples use the values from the quick-start resolver:

| Purpose | Template | Output |
|---------|----------|--------|
| Substitute a tag | `Hello {name}!` | `Hello Linh!` |
| Loop over strings | `{(countries)[{item}]}` | `[Vietnam][Japan]` |
| Access item members | `{(cities){order}: {item.Name}; }` | `1: HCM; 2: Hanoi; ` |
| Use a tag inside a loop | `{(countries){name}: {item}; }` | `Linh: Vietnam; Linh: Japan; ` |
| Escape an opening brace | `{{name}` | `{name}` |
| Keep literal closing braces | `{{name}}` | `{name}}` |

### Tags and literal text

Use `{tagName}` for a resolver value. Names start with a letter or underscore; subsequent characters may also include digits and dots. A dot in an ordinary tag name is passed to your resolver as part of the name; it does not automatically traverse an object.

`{{` produces one literal opening brace. Closing braces outside expressions are preserved, so `}}` is **not** a closing-brace escape.

Spaces, tabs, and line breaks are preserved exactly, including templates containing only whitespace. Null or empty input to `TemplateEngine.Parse` renders an empty string.

### Loops and item members

Use `{(collectionTag)body}` to repeat the body for each value in a collection returned by your resolver. The value must implement `System.Collections.IEnumerable`; a null or non-enumerable value produces no loop output. A string is enumerable and is therefore iterated character by character.

The following names are reserved by the engine and matched case-insensitively:

| Name | Meaning inside a loop |
|------|-----------------------|
| `{item}` | Current collection element |
| `{item.Name}` | Property or field on the current element |
| `{index}` | Zero-based position: 0, 1, 2, ... |
| `{order}` | One-based position: 1, 2, 3, ... |

Outside a loop, `{index}` is `0`, `{order}` is `1`, and `{item}` or an item-member path renders empty text. These reserved names are not sent to your resolver.

Item paths traverse properties and fields one segment at a time with case-insensitive member lookup. For a row whose `Address.City` is `"Hanoi"`, `{(rows){item.Address.City}}` renders `Hanoi`.

### Nested loops and grouped expressions

Nested loops have their own item and position inside their bodies. Following siblings retain the outer loop's context. Loop selectors always use your resolver, not item-relative paths; there is no parent-loop reference syntax.

A non-loop grouped expression inherits the enclosing item, index, and order. For rows named Alice and Bob:

```text
Template: {(rows){ [{index}:{order}:{item.Name}]}}
Output:    [0:1:Alice] [1:2:Bob]
```

The output begins with one space because the group's leading space is literal text.

## Culture-aware formatting

Pass a culture to `ResolveAsync` to format resolver values, whole items, and item members consistently. Using the quick-start resolver:

```csharp
var priceTemplate = TemplateEngine.Parse("{price}");
var formattedPrice = await engine.ResolveAsync(
    priceTemplate, CultureInfo.GetCultureInfo("fr-FR"));

Console.WriteLine(formattedPrice);
// 1,5
```

When no culture is supplied, `CultureInfo.CurrentCulture` is used. Your resolver receives the same effective culture. Strings are not reformatted, and null values render empty text.

## Errors and compatibility notes

### Rendering errors

| Condition | Behavior |
|-----------|----------|
| Null item, intermediate member, or final value | Empty text; `{(rows)[{item.Address.City}]}` renders `[]` for a row with a null address |
| Missing member on a non-null value | `InvalidOperationException` identifying the full item path, failing member, and receiver type |
| Property getter throws | `InvalidOperationException` with member-path context and the original getter exception as `InnerException` |
| Resolver fails | The resolver exception propagates |

Await and handle rendering errors in your application. Failed renders do not return partial successful output.

**Migration:** Missing members previously rendered empty text silently. Correct misspelled paths or provide the expected members before upgrading. Null values are still allowed and render empty text.

### Syntax errors

Every unescaped tag, loop selector, and grouped expression must be complete:

| Invalid input | Correction |
|---------------|------------|
| `{name` | `{name}` |
| `{(rows` | `{(rows){item.Name}}` |
| `{(rows){item.Name}` | `{(rows){item.Name}}` |
| `{ text` | `{ text}` |

Malformed templates throw `FormatException` during `TemplateEngine.Parse`. Diagnostics identify the missing delimiter or invalid loop-selector character and report one-based line and column positions.

End-of-input errors point immediately after the last character: `"x\n{name"` reports line 2, column 6. LF and CRLF each count as one line break; a tab counts as one column. The original parser stack is preserved.

**Migration:** Incomplete expressions previously could parse and produce partial or empty output. Correct these templates before upgrading.

## Rendering limits

Kkts.Template substitutes text as supplied; it does not automatically HTML-encode values. If you generate HTML emails or pages, apply the appropriate encoding to untrusted values in your application.

This is a text-substitution and iteration engine, not a C# or Razor expression evaluator. Keep calculations, conditional logic, and application-specific formatting in your resolver.

## Development

From the repository root, build the library and run the tests:

```sh
dotnet build src/Kkts.Template/Kkts.Template.csproj
dotnet test src/Kkts.Template.UnitTest/Kkts.Template.UnitTest.csproj
```

Tests require the .NET 10 SDK and use xUnit v2 with VSTest. No runtime roll-forward or test-host architecture override is required.

See the [library source](src/Kkts.Template/) and [tests](src/Kkts.Template.UnitTest/) for implementation details and additional examples. Report bugs or propose improvements through [GitHub issues](https://github.com/linhnle/Kkts.Template/issues).

## License and contact

Licensed under the [MIT License](LICENSE).

Author: [Linh Le on LinkedIn](https://www.linkedin.com/in/linh-le-258417105/).
