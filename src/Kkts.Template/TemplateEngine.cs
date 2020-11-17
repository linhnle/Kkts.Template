using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kkts.Template
{
    public sealed class TemplateEngine
    {
        private const string ItemTagNamePrefix = "item.";
        private const string ItemTagName = "item";
        private const string IndexTagName = "index";
        private const string OrderTagName = "order";
        private readonly ITagResolver _resolver;

        public TemplateEngine(ITagResolver resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public static Template Parse(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new Template { Nodes = new List<TemplateNode>(0) };
            }

            var nodes = ParseNodes(content);

            return new Template { Nodes = nodes };
        }

        public Task<string> ResolveAsync(Template template, CultureInfo cultureInfo = null)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            return ResolveTagsAsync(template.Nodes, 0, null, cultureInfo ?? CultureInfo.CurrentCulture);
        }

        private static List<TemplateNode> ParseNodes(string content)
        {
            var result = new List<TemplateNode>();
            var lineCount = 1;
            var column = 1;
            var length = content.Length;
            try
            {
                for (var index = 0; index < length; ++index)
                {
                    var c = content[index];
                    var node = c == Config.StartTag ? new TagNode() as TemplateNode : new TextNode();
                    result.Add(node.Read(content, length, ref index, ref lineCount, ref column));
                }
            }
            catch (FormatException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new FormatException($"Syntax error: line {lineCount}, column {column}", ex);
            }

            return result;
        }

        private async Task<string> ResolveTagsAsync(List<TemplateNode> nodes, int index, object dataItem, CultureInfo cultureInfo)
        {
            var builder = new StringBuilder();
            var cache = new Dictionary<string, object>();
            object value;
            foreach (var node in nodes)
            {
                if (node is TagNode tagNode)
                {
                    if (tagNode.Text.Equals(IndexTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        builder.Append(index);
                        continue;
                    }

                    if (tagNode.Text.Equals(OrderTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        builder.Append(index + 1);
                        continue;
                    }

                    if (ItemTagName.Equals(tagNode.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        if (dataItem is null) continue;
                        builder.Append(Convert.ToString(dataItem, cultureInfo));
                        continue;
                    }

                    if (tagNode.Text.StartsWith(ItemTagNamePrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        if (dataItem is null) continue;
                        value = Resolve(tagNode.Text.Substring(ItemTagNamePrefix.Length), dataItem, cultureInfo);
                        builder.Append(value);
                        continue;
                    }

                    value = await ResolveAsync(tagNode.Text, cache, cultureInfo);
                    builder.Append(value);
                }
                else if (node is NestedExpressionNode nestedNode)
                {
                    if (nestedNode.NestedTagNode != null)
                    {
                        value = await ResolveAsync(nestedNode.NestedTagNode.Text, cache, cultureInfo);
                        if (value is IEnumerable collection)
                        {
                            var i = 0;
                            foreach (var item in collection)
                            {
                                builder.Append(await ResolveTagsAsync(nestedNode.NestedNodes, i++, item, cultureInfo));
                            }
                        }
                    }
                    else
                    {
                        builder.Append(await ResolveTagsAsync(nestedNode.NestedNodes, 0, null, cultureInfo));
                    }
                }
                else
                {
                    builder.Append(node.Text);
                }
            }

            return builder.ToString();
        }

        private async Task<object> ResolveAsync(string tagName, IDictionary<string, object> cache, CultureInfo cultureInfo)
        {
            if (!cache.TryGetValue(tagName, out var value))
            {
                value = await _resolver.ResolveAsync(tagName, cultureInfo);
                cache[tagName] = value;
            }

            return value;
        }

        private string Resolve(string tagName, object dataItem, CultureInfo cultureInfo)
        {
            var segments = tagName.Split('.');
            object tmp = null;
            try
            {
                var param = Expression.Parameter(dataItem.GetType());
                for (var i = 0; i < segments.Length; ++i)
                {
                    var member = Expression.PropertyOrField(param, segments[i])?.Member;
                    switch (member)
                    {
                        case PropertyInfo p:
                            tmp = p.GetValue(dataItem);
                            break;
                        case FieldInfo f:
                            tmp = f.GetValue(dataItem);
                            break;
                    }
                }

                if (tmp is null) return string.Empty;

                return Convert.ToString(tmp, cultureInfo);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
