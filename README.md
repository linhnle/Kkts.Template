# Kkts.Template
Kkts.Template is a .Net template engine for resolving simple template like SMS, emails, or any other formatted text output.

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
## Contacts
**[LinkedIn](https://www.linkedin.com/in/linh-le-258417105/)**
**Skype: linh.nhat.le**

