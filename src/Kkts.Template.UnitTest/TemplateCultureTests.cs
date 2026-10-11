using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace Kkts.Template.UnitTest
{
    public class TemplateCultureTests
    {
        [Theory]
        [InlineData("en-US", "fr-FR", "1,5")]
        [InlineData("fr-FR", null, "1,5")]
        [InlineData("en-US", null, "1.5")]
        public async Task Resolver_items_and_members_use_the_effective_culture(string ambientName, string requestedName, string expected)
        {
            var previousCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(ambientName);
                var requested = requestedName == null ? null : CultureInfo.GetCultureInfo(requestedName);
                var effective = requested ?? CultureInfo.CurrentCulture;
                var resolver = new TestTagResolver(
                    ("price", 1.5m), ("prices", new[] { 1.5m }),
                    ("rows", new[] { new { Price = 1.5m } }), ("text", "1.5"), ("nothing", null));
                var template = TemplateEngine.Parse("{price}|{(prices){item}}|{(rows){item.Price}}|{text}|{nothing}");

                var result = await new TemplateEngine(resolver).ResolveAsync(template, requested);

                Assert.Equal($"{expected}|{expected}|{expected}|1.5|", result);
                Assert.All(resolver.Requests, request => Assert.Same(effective, request.Culture));
                Assert.Equal(ambientName, CultureInfo.CurrentCulture.Name);
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }
        }

        [Fact]
        public async Task Grouped_resolver_values_keep_the_requested_culture()
        {
            var resolver = new TestTagResolver(("price", 1.5m), ("rows", new[] { "row" }));
            var requested = CultureInfo.GetCultureInfo("fr-FR");
            var result = await new TemplateEngine(resolver).ResolveAsync(TemplateEngine.Parse("{(rows){ {price}}}"), requested);
            Assert.Equal(" 1,5", result);
            Assert.All(resolver.Requests, request => Assert.Same(requested, request.Culture));
        }
    }
}
