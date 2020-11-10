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
            var started = false;
            for (; index < length; ++index, ++column)
            {
                var c = content[index];
                if (c == Config.StartTag && !started)
                {
                    started = true;
                    continue;
                }
                else
                {
                    if (isFirstChar)
                    {
                        if (c == UnderScore || char.IsLetter(c))
                        {
                            isFirstChar = false;
                            builder.Append(c);
                        }
                        else
                        {
                            if (c == Config.StartTag)
                            {
                                return new TextNode(c);
                            }

                            var nestedNode = c == Config.StartNestedTag ? new NestedExpressionNode(new NestedTagNode()) : new NestedExpressionNode(new TextNode(builder.ToString()));
                            nestedNode.Read(content, length, ref index, ref lineCount, ref column);

                            return nestedNode;
                        }
                    }
                    else
                    {
                        if (c == UnderScore || c == Dot || char.IsLetter(c) || char.IsDigit(c))
                        {
                            builder.Append(c);
                        }
                        else
                        {
                            if (c == Config.EndTag)
                            {
                                break;
                            }

                            var nestedNode = new NestedExpressionNode(new TextNode(builder.ToString()));
                            nestedNode.Read(content, length, ref index, ref lineCount, ref column);

                            return nestedNode;
                        }
                    }
                }
            }

            Text = builder.ToString();

            return this;
        }
    }
}
