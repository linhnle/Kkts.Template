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
            startNode.StopAtEndTag = true;
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

            if (_startNode != null)
            {
                NestedNodes.Add(_startNode.Read(content, length, ref index, ref lineCount, ref column));
                _startNode = null;
            }

            while (index < length)
            {
                var c = content[index];
                if (c == Config.EndTag)
                {
                    Advance(content, ref index, ref lineCount, ref column);
                    return this;
                }

                var node = c == Config.StartTag ? new TagNode() as TemplateNode : new TextNode() { StopAtEndTag = true };
                NestedNodes.Add(node.Read(content, length, ref index, ref lineCount, ref column));
            }

            throw SyntaxError($"Expected '{Config.EndTag}' before end of input", lineCount, column);
        }
    }
}
