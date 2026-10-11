using System;
using System.Threading.Tasks;
using Xunit;

namespace Kkts.Template.UnitTest
{
    public class TemplateParsingTests
    {
        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        [InlineData(" \n\t", " \n\t")]
        [InlineData("a\r\nb\nc\t ", "a\r\nb\nc\t ")]
        [InlineData("<h1>{{title}</h1>", "<h1>{title}</h1>")]
        [InlineData("<h1>{{title}}</h1>", "<h1>{title}}</h1>")]
        [InlineData("text}}", "text}}")]
        [InlineData("{{", "{")]
        [InlineData("{{{{", "{{")]
        [InlineData("{}", "")]
        [InlineData("{ text}", " text")]
        [InlineData("prefix{foo bar}suffix", "prefixfoo barsuffix")]
        public async Task Literals_groups_and_escapes_preserve_exact_output(string content, string expected)
        {
            var resolver = new TestTagResolver();
            Assert.Equal(expected, await new TemplateEngine(resolver).ResolveAsync(TemplateEngine.Parse(content)));
            Assert.Empty(resolver.Requests);
        }

        [Theory]
        [InlineData("{title", "}")]
        [InlineData("{", "}")]
        [InlineData("{(rows", ")")]
        [InlineData("{(", ")")]
        [InlineData("{(rows)", "}")]
        [InlineData("{(rows){item.Name}", "}")]
        [InlineData("{ text", "}")]
        [InlineData("{(rows){ text}", "}")]
        [InlineData("{(rows){(rows){item}}", "}")]
        [InlineData("{(rows){{", "}")]
        public void Incomplete_expressions_fail_during_parsing(string content, string expectedDelimiter)
        {
            var exception = Assert.Throws<FormatException>(() => TemplateEngine.Parse(content));
            Assert.Contains($"Expected '{expectedDelimiter}'", exception.Message);
            Assert.Contains("end of input", exception.Message);
        }

        [Theory]
        [InlineData("{title}{title}", "TT")]
        [InlineData("{title}tail", "Ttail")]
        [InlineData("{(rows){item.Name}}tail", "AliceBobtail")]
        [InlineData("{(rows){ {item.Name}}}", " Alice Bob")]
        [InlineData("{(rows){(inner){item}}:{item.Name}:{index};}", "X:Alice:0;X:Bob:1;")]
        [InlineData("{(rows){{}", "{{")]
        [InlineData("{(rows){{literal}", "{literal{literal")]
        [InlineData("{(rows){ text}}", " text text")]
        [InlineData("{(rows)}tail", "tail")]
        public async Task Nested_delimiters_and_adjacent_nodes_do_not_skip_characters(string content, string expected)
        {
            var resolver = new TestTagResolver(("title", "T"),
                ("rows", new[] { new { Name = "Alice" }, new { Name = "Bob" } }), ("inner", new[] { "X" }));
            Assert.Equal(expected, await new TemplateEngine(resolver).ResolveAsync(TemplateEngine.Parse(content)));
        }

        [Theory]
        [InlineData("x\n{title", 2, 7, "Expected '}'")]
        [InlineData("x\r\n{title", 2, 7, "Expected '}'")]
        [InlineData("x\n{(rows!)}", 2, 7, "Invalid loop selector character '!'")]
        [InlineData("{title}{broken", 1, 15, "Expected '}'")]
        [InlineData("\t{title", 1, 8, "Expected '}'")]
        [InlineData("{{{broken", 1, 10, "Expected '}'")]
        [InlineData("{(rows)}{broken", 1, 16, "Expected '}'")]
        [InlineData("{ text\n}{broken", 2, 9, "Expected '}'")]
        public void Errors_report_exact_source_positions(string content, int line, int column, string detail)
        {
            var exception = Assert.Throws<FormatException>(() => TemplateEngine.Parse(content));
            Assert.Contains($"line {line}, column {column}.", exception.Message);
            Assert.Contains(detail, exception.Message);
        }

        [Theory]
        [InlineData("{title", "TagNode.Read")]
        [InlineData("{(rows", "NestedTagNode.Read")]
        [InlineData("{(rows)", "NestedExpressionNode.Read")]
        public void Syntax_errors_preserve_the_originating_parser_frame(string content, string frame)
        {
            var exception = Assert.Throws<FormatException>(() => TemplateEngine.Parse(content));
            Assert.Contains(frame, exception.StackTrace);
            Assert.Null(exception.InnerException);
        }
    }
}
