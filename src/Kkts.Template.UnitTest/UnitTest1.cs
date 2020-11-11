using System;
using Xunit;

namespace Kkts.Template.UnitTest
{
    public class UnitTest1
    {
        private static readonly string Template = @"<h1>{title}</h1>
<p>{des}</p>
<p>{{should keep it as it is}}</p>
<h2>Contries</h2>
<table>
{(contries)<tr><td>{order}</td><td>{label}</td><td>{item}</td></tr>}
</table>
<h2>Cities</h2>
<table>
{(cities)<tr><td>{item.Id}</td><td>City:</td><td>{item.Name}</td></tr>}
</table>";
        private static readonly string ExpectedResult = @"<h1>Sample</h1>
<p>this is flyweight template</p>
<p>{should keep it as it is}}</p>
<h2>Contries</h2>
<table>
<tr><td>1</td><td>Contry:</td><td>Vietnam</td></tr><tr><td>2</td><td>Contry:</td><td>Japan</td></tr><tr><td>3</td><td>Contry:</td><td>Korea</td></tr>
</table>
<h2>Cities</h2>
<table>
<tr><td>1</td><td>City:</td><td>HCM</td></tr><tr><td>2</td><td>City:</td><td>Hanoi</td></tr><tr><td>3</td><td>City:</td><td>Danang</td></tr>
</table>";

        [Fact]
        public async void Resolve_Success()
        {
            var tmpl = TemplateEngine.Parse(Template);
            var engine = new TemplateEngine(new TagResolver());
            var result = await engine.ResolveAsync(tmpl);
            Assert.Equal(ExpectedResult, result);
        }
    }
}
