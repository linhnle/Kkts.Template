# Kkts.Template
Kkts.Template is a .Net template engine for resolving simple template like SMS, emails, or any other formatted text output.
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
                return Task.FromResult<object>("this is flyweight template engine");
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
// "<h1>Kk.Template</h1><p>this is flyweight template engine</p>"
```
|Description|Template|Resolved|Note|
|-------------------|-----------------------------------|-----------------------------------|----------------|
|Tag name|<h1>{title}</h1>|<h1>Kk.Template</h1>| title is a tag that is resolved as 'Kk.Template'|
|Escap curly brackets|<h1>{{title}</h1>|<h1>{title}</h1>| title is not a tag, "{{" is resolved as '{'|
|Escap curly brackets|<h1>{{title}}</h1>|<h1>{title}}</h1>| title is not a tag, "{{" is resolved as '{' and all the '}' are kept as they are|
|Loop|{(contries)<li>{item}</li>}|<li>Vietnam</li><li>Japan</li>| contries is an Enumerable<string>|
|Loop|{(cities)<li>{order} - {label}: {item.Name}</li>|<li>1 - City: HCM</li><li>2 - City: Hanoi</li>| cities is an Enumerable<Data>, label is a tag, item is an element of cities|
  
#### Note:
  Some keywords in loop context
  1 - index (count from 0, 1, ..., n)
  2 - order (count from 1, 2, ..., n)
  3 - item (element of collection)
## Contacts
**[LinkedIn](https://www.linkedin.com/in/linh-le-258417105/)**
**Skype: linh.nhat.le**

