using System.Globalization;
using System.Threading.Tasks;

namespace Kkts.Template
{
    public interface ITagResolver
    {
        Task<object> ResolveAsync(string tagName, CultureInfo cultureInfo);
    }
}
