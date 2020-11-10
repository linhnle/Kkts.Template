using System.Text;

namespace Kkts.Template
{
    internal sealed class TextNode : TemplateNode
    {
        internal bool StopAtEndTag;

        public TextNode() { }

        public TextNode(char specialChar) => Text = specialChar.ToString();

        public TextNode(string prefix) => Text = prefix;

        public override TemplateNode Read(string content, int length, ref int index, ref int lineCount, ref int column)
        {
            var builder = new StringBuilder();
            builder.Append(Text);
            for (; index < length; ++index, ++column)
            {
                var c = content[index];
                if (StopAtEndTag && c == Config.EndTag) break;
                if (c == Config.StartTag)
                {
                    --index;
                    break;
                }
                else
                {
                    if (c == Config.NewLine)
                    {
                        ++lineCount;
                        column = 0;
                    }

                    builder.Append(c);
                }
            }

            Text = builder.ToString();

            return this;
        }
    }
}
