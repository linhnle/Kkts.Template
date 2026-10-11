using System.Text;

namespace Kkts.Template
{
    internal sealed class TagNode : TemplateNode
    {
        private const char UnderScore = '_';
        private const char Dot = '.';

        public override TemplateNode Read(string content, int length, ref int index, ref int lineCount, ref int column)
        {
            var builder = new StringBuilder();
            var isFirstChar = true;
            Advance(content, ref index, ref lineCount, ref column);
            if (index < length && content[index] == Config.StartTag)
            {
                Advance(content, ref index, ref lineCount, ref column);
                return new TextNode(Config.StartTag);
            }

            while (index < length)
            {
                var c = content[index];
                var isNameChar = c == UnderScore || char.IsLetter(c) || (!isFirstChar && (c == Dot || char.IsDigit(c)));
                if (isNameChar)
                {
                    isFirstChar = false;
                    builder.Append(c);
                    Advance(content, ref index, ref lineCount, ref column);
                    continue;
                }

                if (!isFirstChar && c == Config.EndTag)
                {
                    Advance(content, ref index, ref lineCount, ref column);
                    Text = builder.ToString();
                    return this;
                }

                var nestedNode = isFirstChar && c == Config.StartNestedTag
                    ? new NestedExpressionNode(new NestedTagNode())
                    : new NestedExpressionNode(new TextNode(builder.ToString()));
                return nestedNode.Read(content, length, ref index, ref lineCount, ref column);
            }

            throw SyntaxError($"Expected '{Config.EndTag}' before end of input", lineCount, column);
        }
    }
}
