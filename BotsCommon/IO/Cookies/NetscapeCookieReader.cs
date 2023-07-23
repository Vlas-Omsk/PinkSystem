using System.Diagnostics;
using System.Net;

namespace BotsCommon.IO.Cookies
{
    public sealed class NetscapeCookieReader : ICookieReader
    {
        public string Domain { get; set; }
        public bool UseExpirationTimestamp { get; set; }

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

        public IEnumerable<Cookie> ReadAllCookies(string path)
        {
            string line;

            using var reader = new StreamReader(path);

            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split("\t");

                var cookieDomain = parts[0];

                if (Domain != null && !cookieDomain.EndsWith(Domain))
                    continue;

                var name = parts[5];

                if (string.IsNullOrEmpty(name))
                    continue;

                if (cookieDomain[0] == '[' && cookieDomain[cookieDomain.Length - 1] == ']')
                {
                    Debug.Print("Skipped cookie with ipv6 domain.");
                    continue;
                }

                var cookie = new Cookie()
                {
                    Domain = cookieDomain,
                    Path = parts[2],
                    Secure = bool.Parse(parts[3]),
                    Name = name,
                    Value = '"' + parts[6] + '"'
                };

                if (UseExpirationTimestamp)
                    cookie.Expires = new UnixTimestamp(long.Parse(parts[4])).DateTime;

                yield return cookie;
            }
        }
    }
}
