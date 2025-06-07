using System.Text.RegularExpressions;
using PinkSystem.IO.Data;

namespace PinkSystem.Net
{
    public static class IDataReaderExtensions
    {
        public static IDataReader<Proxy> ConvertToProxy(this IDataReader<string> self, ProxyProtocol? defaultProtocol = null)
        {
            return self.Convert(x => Proxy.Parse(x, defaultProtocol));
        }

        public static IDataReader<Proxy> ConvertToProxy(this IDataReader<string> self, Regex format, ProxyProtocol? defaultProtocol = null)
        {
            return self.Convert(x => Proxy.Parse(x, format, defaultProtocol));
        }
    }
}
