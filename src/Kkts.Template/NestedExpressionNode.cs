using System.Collections.Generic;

namespace Kkts.Template
{
    internal class NestedExpressionNode : TemplateNode
    {
        private TemplateNode _startNode;
        public readonly List<TemplateNode> NestedNodes = new List<TemplateNode>();
        public NestedTagNode NestedTagNode { get; set; }

        public NestedExpressionNode(TextNode startNode)
        {
            _startNode = startNode;
        }

        public NestedExpressionNode(NestedTagNode nestedTagNode)
        {
            NestedTagNode = nestedTagNode;
        }

        public override TemplateNode Read(string content, int length, ref int index, ref int lineCount, ref int column)
        {
            if (NestedTagNode != null)
            {
                NestedTagNode.Read(content, length, ref index, ref lineCount, ref column);
            }

            var stack = new Stack<char>();
            stack.Push(Config.StartTag);
            bool ShouldBreak()
            {
                stack.Pop();
                return stack.Count == 0;
            }

            for (; index < length; ++index)
            {
                if (_startNode != null)
                {
                    NestedNodes.Add(_startNode.Read(content, length, ref index, ref lineCount, ref column));
                    _startNode = null;
                }
                else
                {
                    var c = content[index];
                    var isStartTag = c == Config.StartTag;
                    var isEndTag = c == Config.EndTag;
                    if (isStartTag)
                    {
                        stack.Push(Config.StartTag);
                    }

                    if (isEndTag && ShouldBreak())
                    {
                        break;
                    }
                    var node = isStartTag ? new TagNode() as TemplateNode : new TextNode() { StopAtEndTag = true };
                    NestedNodes.Add(node.Read(content, length, ref index, ref lineCount, ref column));

                    isEndTag = index < length && content[index] == Config.EndTag;
                    if (isEndTag && ShouldBreak())
                    {
                        break;
                    }
                }
            }

            return this;
        }
    }
}
