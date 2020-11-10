using System.Collections.Generic;

namespace Kkts.Template
{
    public sealed class Template
    {
        internal Template() { }

        public string Name { get; set; }

        internal List<TemplateNode> Nodes { get; set; }
    }
}
