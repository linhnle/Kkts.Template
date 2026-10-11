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
            while (index < length)
            {
                var c = content[index];
                if (c == Config.StartTag || (StopAtEndTag && c == Config.EndTag)) break;

                builder.Append(c);
                Advance(content, ref index, ref lineCount, ref column);
            }

            Text = builder.ToString();

            return this;
        }
    }
}
