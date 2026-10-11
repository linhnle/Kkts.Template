using System;

namespace Kkts.Template
{
    internal abstract class TemplateNode
    {
        public string Text { get; protected set; }

        // Readers consume their delimiters and leave index at the next unread character.
        public abstract TemplateNode Read(string content, int length, ref int index, ref int lineCount, ref int column);

        protected static void Advance(string content, ref int index, ref int lineCount, ref int column)
        {
            if (content[index++] == Config.NewLine)
            {
                ++lineCount;
                column = 1;
            }
            else
            {
                ++column;
            }
        }

        protected static FormatException SyntaxError(string detail, int lineCount, int column)
        {
            return new FormatException($"Syntax error: line {lineCount}, column {column}. {detail}");
        }
    }
}
