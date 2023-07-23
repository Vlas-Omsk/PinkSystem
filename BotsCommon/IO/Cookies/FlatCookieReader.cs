using PinkNet;
using System.Net;

namespace BotsCommon.IO.Cookies
{
    public sealed class FlatCookieReader : ICookieReader
    {
        public FlatCookieReader(string domain)
        {
            Domain = domain;
        }

        public string Domain { get; }

        public bool IsFileFormatSupported(string path)
        {
            using var reader = new StreamReader(path);

            var line = reader.ReadLine();

            if (line == null)
                return false;

            line = reader.ReadLine();

            if (line != null)
                return false;

            return true;
        }

        public IEnumerable<Cookie> ReadAllCookies(string path)
        {
            using var reader = new StreamReader(path);

            var line = reader.ReadLine();
            var cookies = CookieUtils.ParseHttpRequestCookieString(line);

            foreach (var cookie in cookies)
            {
                cookie.Domain = Domain;

                yield return cookie;
            }
        }
    }
}
