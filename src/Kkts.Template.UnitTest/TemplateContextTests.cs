using System.Threading.Tasks;
using Xunit;

namespace Kkts.Template.UnitTest
{
    public class TemplateContextTests
    {
        [Fact]
        public async Task Group_inherits_the_current_item()
        {
            var resolver = new TestTagResolver(("rows", new[] { new { Name = "Alice" } }));
            Assert.Equal(" Alice", await new TemplateEngine(resolver).ResolveAsync(TemplateEngine.Parse("{(rows){ {item.Name}}}")));
        }

        [Fact]
        public async Task Group_inherits_index_and_order_for_each_item()
        {
            var resolver = new TestTagResolver(("rows", new[] { new { Name = "Alice" }, new { Name = "Bob" } }));
            var result = await new TemplateEngine(resolver).ResolveAsync(TemplateEngine.Parse("{(rows){ [{index}:{order}:{item.Name}]}}"));
            Assert.Equal(" [0:1:Alice] [1:2:Bob]", result);
        }

        [Fact]
        public async Task Nested_loop_does_not_replace_outer_sibling_context()
        {
            var resolver = new TestTagResolver(("rows", new[] { new { Name = "Alice" }, new { Name = "Bob" } }), ("inner", new[] { "X" }));
            var result = await new TemplateEngine(resolver).ResolveAsync(TemplateEngine.Parse("{(rows){(inner){item}}:{item.Name}:{index};}"));
            Assert.Equal("X:Alice:0;X:Bob:1;", result);
        }
    }
}
