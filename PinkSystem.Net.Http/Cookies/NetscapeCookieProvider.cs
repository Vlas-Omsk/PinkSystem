using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using PinkSystem.IO.Content;

namespace PinkSystem.Net.Http.Cookies
{
    public sealed class NetscapeCookieProvider : ICookieProvider
    {
        public string? Domain { get; set; }
        public bool UseExpirationTimestamp { get; set; } = false;

        public bool IsSupported(IContentReader reader)
        {
            using var streamReader = new StreamReader(reader.CreateStream());

            var line = streamReader.ReadLine();

            if (line == null)
                return false;

            var parts = line.Split("\t");

            if (parts.Length != 7)
                return false;

            return true;
        }

        public IEnumerable<Cookie> ReadAll(IContentReader reader)
        {
            string? line;

            using var streamReader = new StreamReader(reader.CreateStream());

            while ((line = streamReader.ReadLine()) != null)
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

                Cookie cookie;

                try
                {
                    cookie = new Cookie()
                    {
                        Domain = cookieDomain,
                        Path = parts[2],
                        Secure = bool.Parse(parts[3]),
                        Name = name,
                        Value = parts[6]
                    };
                }
                catch
                {
                    cookie = new Cookie()
                    {
                        Domain = cookieDomain,
                        Path = parts[2],
                        Secure = bool.Parse(parts[3]),
                        Name = name,
                        Value = '"' + parts[6] + '"'
                    };
                }

                if (UseExpirationTimestamp)
                    cookie.Expires = UnixTimestamp.FromSeconds(long.Parse(parts[4])).DateTime;

                yield return cookie;
            }
        }
    }
}
