namespace Kkts.Template
{
    internal abstract class TemplateNode
    {
        public string Text { get; protected set; }

        public abstract TemplateNode Read(string content, int length, ref int index, ref int lineCount, ref int column);
    }
}
