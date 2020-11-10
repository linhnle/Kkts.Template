using System;
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
            var started = false;
            for (; index < length; ++index, ++column)
            {
                var c = content[index];
                if (c == Config.StartNestedTag && !started)
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
                            throw new FormatException($"Syntax error: line {lineCount}, column {column}");
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
                            if (c == Config.EndNestedTag)
                            {
                                ++index;
                                break;
                            }

                            throw new FormatException($"Syntax error: line {lineCount}, column {column}");
                        }
                    }
                }
            }

            Text = builder.ToString();

            return this;
        }
    }
}
