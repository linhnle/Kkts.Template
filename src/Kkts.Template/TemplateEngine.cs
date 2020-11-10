using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

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

        public string Resolve(Template template, CultureInfo cultureInfo = null)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            return ResolveTags(template.Nodes, 0, null, cultureInfo ?? CultureInfo.CurrentCulture);
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

        private string ResolveTags(List<TemplateNode> nodes, int index, object dataItem, CultureInfo cultureInfo)
        {
            var builder = new StringBuilder();
            foreach (var node in nodes)
            {
                if (node is TagNode tagNode)
                {
                    if (tagNode.Text.Equals(IndexTagName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        builder.Append(index);
                        continue;
                    }

                    if (tagNode.Text.Equals(OrderTagName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        builder.Append(index + 1);
                        continue;
                    }

                    if (ItemTagName.Equals(tagNode.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (dataItem is null) continue;
                        builder.Append(Convert.ToString(dataItem, cultureInfo));
                        continue;
                    }

                    if (tagNode.Text.StartsWith(ItemTagNamePrefix, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (dataItem is null) continue;
                        var value = Resolve(tagNode.Text.Substring(ItemTagNamePrefix.Length), dataItem, cultureInfo);
                        builder.Append(value);
                        continue;
                    }

                    builder.Append(_resolver.Resolve(tagNode.Text, cultureInfo));
                }
                else if (node is NestedExpressionNode nestedNode)
                {
                    if (nestedNode.NestedTagNode != null)
                    {
                        var data = _resolver.Resolve(nestedNode.NestedTagNode.Text, cultureInfo);
                        if (data is IEnumerable collection)
                        {
                            var i = 0;
                            foreach (var item in collection)
                            {
                                builder.Append(ResolveTags(nestedNode.NestedNodes, i++, item, cultureInfo));
                            }
                        }
                    }
                    else
                    {
                        builder.Append(ResolveTags(nestedNode.NestedNodes, 0, null, cultureInfo));
                    }
                }
                else
                {
                    builder.Append(node.Text);
                }
            }

            return builder.ToString();
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
                    tmp = ((dynamic)member).GetValue(dataItem);
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
