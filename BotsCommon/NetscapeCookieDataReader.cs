using System;
using System.Net;
using BotsCommon.IO;

namespace BotsCommon
{
    public sealed class NetscapeCookieReader
    {
        private readonly IDataReader<string> _reader;

        public NetscapeCookieReader(IDataReader<string> reader)
        {
            _reader = reader;
        }

        public IEnumerable<Cookie> GetAllCookies(string domain = null, bool? overrideSecure = null, bool useExpirationTimestamp = false)
        {
            string line;

            while ((line = _reader.Read()) != null)
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
                    Secure = overrideSecure ?? bool.Parse(parts[3]),
                    Name = name,
                    Value = parts[6]
                };

                if (useExpirationTimestamp)
                    cookie.Expires = TimeConverter.FromUnixTimestamp(long.Parse(parts[4]));

                yield return cookie;
            }
        }
    }
}
