# Kkts.Template
Kkts.Template is a .Net template engine for resolving simple template like SMS, emails, or any other formatted text output.

## No regular expressions

get via nuget **[Kkts.Expressions](https://www.nuget.org/packages/Kkts.Template/)** 
### Usage
``` csharp
class TagResolver : ITagResolver
{
    public Task<object> ResolveAsync(string tagName, CultureInfo cultureInfo)
    {
        switch (tagName.ToLower())
        {
            case "title":
                return Task.FromResult<object>("Kk.Template");
            case "des":
                return Task.FromResult<object>("this is light-weight template engine");
            case "label":
                return Task.FromResult<object>("City");
            case "contries":
                return Task.FromResult<object>(new[] { "Vietnam", "Japan" });
            case "cities":
                return Task.FromResult<object>(new[] { new Data { Id = 1, Name = "HCM" }, new Data { Id = 2, Name = "Hanoi" } });
            default:
                return Task.FromResult<object>(string.Empty);
        }
    }
}

var tmpl = TemplateEngine.Parse("<h1>{title}</h1><p>{des}</p>");
var engine = new TemplateEngine(new TagResolver());
var result = await engine.ResolveAsync(tmpl);
// "<h1>Kk.Template</h1><p>this is light-weight template engine</p>"
```
|Description|Template|Resolved|Note|
|-------------------|-----------------------------------|-----------------------------------|----------------|
|Tag name|&lt;h1&gt;{title}&lt;/h1&gt;|&lt;h1&gt;Kk.Template&lt;/h1&gt;| title is a tag that is resolved as 'Kk.Template'|
|Escape curly brackets|&lt;h1&gt;{{title}&lt;/h1&gt;|&lt;h1&gt;{title}&lt;/h1&gt;| title is not a tag, "{{" is resolved as '{'|
|Escape curly brackets|&lt;h1&gt;{{title}}&lt;/h1&gt;|&lt;h1&gt;{title}}&lt;/h1&gt;| title is not a tag, "{{" is resolved as '{' and all the '}' are kept as they are|
|Loop|{(contries)&lt;li&gt;{item}&lt;/li&gt;}|&lt;li&gt;Vietnam&lt;/li&gt;&lt;li&gt;Japan&lt;/li&gt;| contries is an Enumerable&lt;string&gt;|
|Loop|{(cities)&lt;li&gt;{order} - {label}: {item.Name}&lt;/li&gt;}|&lt;li&gt;1 - City: HCM&lt;/li&gt;&lt;li&gt;2 - City: Hanoi&lt;/li&gt;| cities is an Enumerable&lt;Data&gt;, label is a tag, item is an element of cities|
  
#### Note:
  Some keywords in loop context
  
  1 - index (count from 0, 1, ..., n)
  
  2 - order (count from 1, 2, ..., n)
  
  3 - item (element of collection)
## Item members and errors

Inside a loop, paths traverse properties and fields one segment at a time. For a row whose `Address.City` is `"Hanoi"`, `{(rows){item.Address.City}}` renders `Hanoi`. Existing case-insensitive member lookup is retained.

| Condition | Behavior |
|-----------|----------|
| Null item, intermediate member, or final value | Empty text; for example, `{(rows)[{item.Address.City}]}` renders `[]` for a row with a null address |
| Missing member on a non-null value | `InvalidOperationException` identifying the full item path, failing member, and receiver type |
| Property getter throws | `InvalidOperationException` with member-path context and the original getter exception as `InnerException` |
| External tag resolver fails | The resolver exception propagates |

**Migration:** Missing members previously rendered empty text silently. Correct misspelled paths or provide the expected members before upgrading. A null value is still allowed and renders empty. Await and handle render failures at the calling application; failed renders do not return partial successful output.

## Culture and grouped expressions

Pass a culture to `ResolveAsync` to format resolver values, whole items, and item members consistently:

```csharp
// If "price" resolves to decimal 1.5m:
var result = await engine.ResolveAsync(
    TemplateEngine.Parse("{price}"), CultureInfo.GetCultureInfo("fr-FR"));
// "1,5", even when the current culture is en-US
```

When no culture is supplied, the current culture is used. The resolver receives the same effective culture. String values are not reformatted, and null resolver values render empty text.

A non-loop grouped expression inherits the enclosing item, index, and order. With rows named Alice and Bob, `{(rows){ [{index}:{order}:{item.Name}]}}` renders ` [0:1:Alice] [1:2:Bob]`.

Nested loops have their own item and index inside their bodies; following siblings retain the outer context. Loop selectors still use the external resolver, not item-relative paths. No parent-loop reference syntax is introduced.

## Literal text and syntax diagnostics

Literal whitespace is preserved exactly, including templates containing only spaces, tabs, or line breaks. Null and empty template input still render as an empty string. `{{` escapes an opening brace; literal closing braces outside expressions remain unchanged, as in the examples above.

Every unescaped tag, loop selector, and grouped expression must be complete:

| Invalid input | Correction |
|---------------|------------|
| `{title` | `{title}` |
| `{(rows` | `{(rows){item.Name}}` |
| `{(rows){item.Name}` | `{(rows){item.Name}}` |
| `{ text` | `{ text}` |

**Migration:** Incomplete expressions previously could parse and produce partial or empty output. They now throw `FormatException` during `TemplateEngine.Parse`; correct these templates before upgrading.

Syntax errors identify the missing delimiter or invalid loop-selector character and report one-based line and column positions. End-of-input errors point immediately after the last character: `"x\n{title"` reports line 2, column 7. LF and CRLF each count as one line break, and a tab counts as one column. The original parser stack is preserved.

## Development

The library targets `netstandard2.0`. Tests require the .NET 10 SDK and run with xUnit v2 and VSTest:

```sh
dotnet test src/Kkts.Template.UnitTest/Kkts.Template.UnitTest.csproj
dotnet build src/Kkts.Template/Kkts.Template.csproj
```

No runtime roll-forward or test-host architecture override is required.

## Contacts
**[LinkedIn](https://www.linkedin.com/in/linh-le-258417105/)**
