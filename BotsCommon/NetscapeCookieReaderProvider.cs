using System;
using System.Net;
using System.Web;

namespace BotsCommon
{
    public sealed class NetscapeCookieReaderProvider : ICookieReaderProvider
    {
        public bool IsFileFormatSupported(string path)
        {
            using var reader = new StreamReader(path);

            var line = reader.ReadLine();

            if (line == null)
                return false;

            var parts = line.Split("\t");

            if (parts.Length != 7)
                return false;

            return true;
        }

        public IEnumerable<Cookie> ReadAllCookies(string path, string domain = null, bool useExpirationTimestamp = false)
        {
            string line;

            using var reader = new StreamReader(path);

            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split("\t");

                var cookieDomain = parts[0];

                if (domain != null && !cookieDomain.EndsWith(domain))
                    continue;

                var name = parts[5];

                if (string.IsNullOrEmpty(name))
                    continue;

                var cookie = new Cookie()
                {
                    Domain = cookieDomain,
                    Path = parts[2],
                    Secure = bool.Parse(parts[3]),
                    Name = name,
                    Value = HttpUtility.UrlEncode(parts[6])
                };

                if (useExpirationTimestamp)
                    cookie.Expires = TimeConverter.FromUnixTimestamp(long.Parse(parts[4]));

                yield return cookie;
            }
        }
    }
}
