using System.Globalization;

namespace Kkts.Template
{
    public interface ITagResolver
    {
        object Resolve(string tagName, CultureInfo cultureInfo);
    }
}
