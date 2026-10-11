using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Kkts.Template.UnitTest
{
    public class TemplateCompatibilityTests
    {
        [Theory]
        [InlineData("item.Name", "Alice")]
        [InlineData("ITEM.name", "Alice")]
        [InlineData("item.Id", "1")]
        [InlineData("item.id", "1")]
        public async Task Inherited_members_and_case_matching_are_preserved(string path, string expected)
        {
            var resolver = new TestTagResolver(("rows", new[] { new DerivedRow() }));
            var result = await new TemplateEngine(resolver).ResolveAsync(TemplateEngine.Parse("{(rows){" + path + "}}"));
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Reserved_keywords_keep_their_top_level_behavior()
        {
            var resolver = new TestTagResolver();
            var result = await new TemplateEngine(resolver).ResolveAsync(TemplateEngine.Parse("{INDEX}:{ORDER}:[{ITEM}][{item.Name}]"));
            Assert.Equal("0:1:[][]", result);
            Assert.Empty(resolver.Requests);
        }

        [Fact]
        public async Task Cache_is_local_to_each_render_scope_and_call()
        {
            var resolver = new TestTagResolver(("title", "T"), ("rows", new[] { 1, 2 }));
            var template = TemplateEngine.Parse("{title}{title}{(rows){title}{title}}{title}");
            var engine = new TemplateEngine(resolver);

            Assert.Equal("TTTTTTT", await engine.ResolveAsync(template));
            Assert.Equal(3, resolver.Requests.Count(request => request.Tag == "title"));
            Assert.Equal("TTTTTTT", await engine.ResolveAsync(template));
            Assert.Equal(6, resolver.Requests.Count(request => request.Tag == "title"));
        }

        [Fact]
        public async Task Resolver_tag_cache_remains_case_sensitive()
        {
            var resolver = new TestTagResolver(("title", "a"), ("TITLE", "b"));
            var result = await new TemplateEngine(resolver).ResolveAsync(TemplateEngine.Parse("{title}{TITLE}{title}"));
            Assert.Equal("aba", result);
            Assert.Equal(2, resolver.Requests.Count);
        }

        [Theory]
        [InlineData("<h1>{title}</h1>", "<h1>Sample</h1>")]
        [InlineData("{(countries)<li>{order}:{item}</li>}", "<li>1:Vietnam</li><li>2:Japan</li>")]
        [InlineData("{(empty){item}}", "")]
        public async Task Documented_templates_keep_their_output(string content, string expected)
        {
            var resolver = new TestTagResolver(("title", "Sample"), ("countries", new[] { "Vietnam", "Japan" }), ("empty", new object[0]));
            Assert.Equal(expected, await new TemplateEngine(resolver).ResolveAsync(TemplateEngine.Parse(content)));
        }

        public class BaseRow
        {
            public string Name { get; set; } = "Alice";
            public int Id = 1;
        }

        public sealed class DerivedRow : BaseRow { }
    }
}
