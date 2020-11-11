using System.Globalization;
using System.Threading.Tasks;

namespace Kkts.Template.UnitTest
{
    class TagResolver : ITagResolver
    {
        public Task<object> ResolveAsync(string tagName, CultureInfo cultureInfo)
        {
            switch (tagName.ToLower())
            {
                case "title":
                    return Task.FromResult<object>("Sample");
                case "des":
                    return Task.FromResult<object>("this is flyweight template");
                case "label":
                    return Task.FromResult<object>("Contry:");
                case "contries":
                    return Task.FromResult<object>(new[] { "Vietnam", "Japan", "Korea" });
                case "cities":
                    return Task.FromResult<object>(new[] { new Data { Id = 1, Name = "HCM" }, new Data { Id = 2, Name = "Hanoi" }, new Data { Id = 3, Name = "Danang" } });
                default:
                    return Task.FromResult<object>(string.Empty);
            }
        }
    }

    class Data
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
