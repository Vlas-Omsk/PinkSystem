using System;
using System.Net;
using System.Web;
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

        public IEnumerable<Cookie> GetAllCookies(string domain)
        {
            if (domain == null)
                yield break;

            string line;

            while ((line = _reader.Read()) != null)
            {
                var parts = line.Split("\t");

                var cookieDomain = parts[0];

                if (!cookieDomain.EndsWith(domain))
                    continue;

                var name = parts[5];

                if (string.IsNullOrEmpty(name))
                    continue;

                yield return new Cookie()
                {
                    Domain = cookieDomain,
                    Path = parts[2],
                    Secure = bool.Parse(parts[3]),
                    Expires = TimeConverter.FromUnixTimestamp(long.Parse(parts[4])),
                    Name = HttpUtility.UrlEncode(name),
                    Value = HttpUtility.UrlEncode(parts[6])
                };
            }
        }
    }
}
