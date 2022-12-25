using System;
using System.Net;

namespace BotsCommon.Net
{
    public interface ICookieReaderProvider
    {
        bool IsFileFormatSupported(string path);
        IEnumerable<Cookie> ReadAllCookies(string path, string domain = null, bool useExpirationTimestamp = false);
    }
}
