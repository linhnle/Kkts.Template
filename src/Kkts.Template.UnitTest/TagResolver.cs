using System.Globalization;

namespace Kkts.Template.UnitTest
{
    class TagResolver : ITagResolver
    {
        public object Resolve(string tagName, CultureInfo cultureInfo)
        {
            switch (tagName.ToLower())
            {
                case "title":
                    return "Sample";
                case "des":
                    return "this is flyweight template";
                case "label":
                    return "Contry:";
                case "contries":
                    return new[] { "Vietnam", "Japan", "Korea" };
                case "cities":
                    return new[] { new Data { Id = 1, Name = "HCM" }, new Data { Id = 2, Name = "Hanoi" }, new Data { Id = 3, Name = "Danang" } };
                default:
                    return string.Empty;
            }
        }
    }

    class Data
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
