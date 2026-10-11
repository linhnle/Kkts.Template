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
            if (string.IsNullOrEmpty(content))
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
            var index = 0;
            while (index < length)
            {
                var c = content[index];
                var node = c == Config.StartTag ? new TagNode() as TemplateNode : new TextNode();
                result.Add(node.Read(content, length, ref index, ref lineCount, ref column));
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
                        value = Resolve(tagNode.Text, dataItem, cultureInfo);
                        builder.Append(value);
                        continue;
                    }

                    value = await ResolveAsync(tagNode.Text, cache, cultureInfo);
                    builder.Append(Convert.ToString(value, cultureInfo));
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
                        builder.Append(await ResolveTagsAsync(nestedNode.NestedNodes, index, dataItem, cultureInfo));
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
            var segments = tagName.Substring(ItemTagNamePrefix.Length).Split('.');
            var current = dataItem;
            foreach (var segment in segments)
            {
                if (current is null) return string.Empty;

                var receiverType = current.GetType();
                var context = $"Cannot resolve '{tagName}': member '{segment}' on type '{receiverType.FullName}'";
                MemberInfo member;
                try
                {
                    member = Expression.PropertyOrField(Expression.Parameter(receiverType), segment).Member;
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidOperationException(context, ex);
                }

                try
                {
                    switch (member)
                    {
                        case PropertyInfo p:
                            current = p.GetValue(current);
                            break;
                        case FieldInfo f:
                            current = f.GetValue(current);
                            break;
                    }
                }
                catch (TargetInvocationException ex) when (ex.InnerException != null)
                {
                    throw new InvalidOperationException(context, ex.InnerException);
                }
            }

            return Convert.ToString(current, cultureInfo);
        }
    }
}
