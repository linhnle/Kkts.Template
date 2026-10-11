using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace Kkts.Template.UnitTest
{
    public class TemplateMemberTests
    {
        [Fact]
        public async Task Nested_properties_use_each_successive_receiver()
        {
            Assert.Equal("Hanoi", await Render(new PropertyRow { Address = new Address { City = "Hanoi" } }, "{item.Address.City}"));
        }

        [Fact]
        public async Task Mixed_fields_and_properties_use_each_successive_receiver()
        {
            Assert.Equal("Hanoi", await Render(new FieldRow { Address = new Address { City = "Hanoi" } }, "{item.Address.City}"));
        }

        [Fact]
        public async Task Single_segment_properties_and_fields_remain_supported()
        {
            Assert.Equal("1:Alice", await Render(new TemplateCompatibilityTests.DerivedRow(), "{item.Id}:{item.Name}"));
        }

        [Fact]
        public async Task Null_items_intermediate_values_and_leaf_values_render_empty()
        {
            Assert.Equal("[]", await Render(null, "[{item.Name}]"));
            Assert.Equal("[]", await Render(new PropertyRow(), "[{item.Address.City}]"));
            Assert.Equal("[]", await Render(new PropertyRow { Address = new Address() }, "[{item.Address.City}]"));
        }

        [Theory]
        [InlineData("item.Missing", "Missing")]
        [InlineData("item.Address.Missing", "Missing")]
        public async Task Missing_members_fault_the_render_with_context(string path, string segment)
        {
            var row = new PropertyRow { Address = new Address() };
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Render(row, "prefix{" + path + "}suffix"));
            Assert.Contains(path, exception.Message);
            Assert.Contains(segment, exception.Message);
            Assert.Contains(path == "item.Missing" ? typeof(PropertyRow).FullName : typeof(Address).FullName, exception.Message);
        }

        [Fact]
        public async Task Missing_city_on_non_null_address_reports_the_full_path_and_receiver()
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Render(new { Address = new { Country = "Vietnam" } }, "{item.Address.City}"));
            Assert.Contains("item.Address.City", exception.Message);
            Assert.Contains("member 'City'", exception.Message);
            Assert.Contains(new { Country = "Vietnam" }.GetType().FullName, exception.Message);
        }

        [Fact]
        public async Task Getter_failure_preserves_the_original_exception()
        {
            var original = new ApplicationException("getter failed");
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Render(new ThrowingRow(original), "prefix{item.Name}suffix"));
            Assert.Contains("item.Name", exception.Message);
            Assert.Contains("member 'Name'", exception.Message);
            Assert.Contains(typeof(ThrowingRow).FullName, exception.Message);
            Assert.Same(original, exception.InnerException);
        }

        [Fact]
        public async Task Resolver_failures_propagate_without_empty_output()
        {
            var original = new ApplicationException("resolver failed");
            var engine = new TemplateEngine(new FailingResolver(original));
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => engine.ResolveAsync(TemplateEngine.Parse("prefix{title}suffix")));
            Assert.Same(original, exception);
        }

        private static Task<string> Render(object row, string body)
        {
            var resolver = new TestTagResolver(("rows", new[] { row }));
            return new TemplateEngine(resolver).ResolveAsync(TemplateEngine.Parse("{(rows)" + body + "}"));
        }

        public sealed class Address
        {
            public string City { get; set; }
        }

        public sealed class PropertyRow
        {
            public Address Address { get; set; }
        }

        public sealed class FieldRow
        {
            public Address Address;
        }

        public sealed class ThrowingRow
        {
            private readonly Exception _exception;
            public ThrowingRow(Exception exception) => _exception = exception;
            public string Name => throw _exception;
        }

        private sealed class FailingResolver : ITagResolver
        {
            private readonly Exception _exception;
            public FailingResolver(Exception exception) => _exception = exception;
            public Task<object> ResolveAsync(string tagName, CultureInfo cultureInfo) => Task.FromException<object>(_exception);
        }
    }
}
