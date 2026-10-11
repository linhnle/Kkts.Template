using System.Text;

namespace Kkts.Template
{
    internal sealed class NestedTagNode : TemplateNode
    {
        private const char UnderScore = '_';
        private const char Dot = '.';

        public override TemplateNode Read(string content, int length, ref int index, ref int lineCount, ref int column)
        {
            var builder = new StringBuilder();
            var isFirstChar = true;
            Advance(content, ref index, ref lineCount, ref column);
            while (index < length)
            {
                var c = content[index];
                if (c == UnderScore || char.IsLetter(c) || (!isFirstChar && (c == Dot || char.IsDigit(c))))
                {
                    isFirstChar = false;
                    builder.Append(c);
                    Advance(content, ref index, ref lineCount, ref column);
                    continue;
                }

                if (!isFirstChar && c == Config.EndNestedTag)
                {
                    Advance(content, ref index, ref lineCount, ref column);
                    Text = builder.ToString();
                    return this;
                }

                throw SyntaxError($"Invalid loop selector character '{c}'", lineCount, column);
            }

            throw SyntaxError($"Expected '{Config.EndNestedTag}' before end of input", lineCount, column);
        }
    }
}
