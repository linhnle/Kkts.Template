using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Kkts.Template.UnitTest
{
    internal sealed class TestTagResolver : ITagResolver
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        public List<(string Tag, CultureInfo Culture)> Requests { get; } = new List<(string, CultureInfo)>();

        public TestTagResolver(params (string Tag, object Value)[] values)
        {
            foreach (var value in values)
            {
                _values.Add(value.Tag, value.Value);
            }
        }

        public Task<object> ResolveAsync(string tagName, CultureInfo cultureInfo)
        {
            Requests.Add((tagName, cultureInfo));
            return Task.FromResult(_values[tagName]);
        }
    }
}
