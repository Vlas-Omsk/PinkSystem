using System.Net;

namespace BotsCommon.IO.Cookies
{
    public interface ICookieReader
    {
        bool IsFileFormatSupported(string path);
        IEnumerable<Cookie> ReadAllCookies(string path);
    }
}
