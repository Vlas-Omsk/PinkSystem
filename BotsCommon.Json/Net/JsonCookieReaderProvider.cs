using System;
using System.Net;
using PinkJson2;

namespace BotsCommon.Net
{
    public sealed class JsonCookieReaderProvider : ICookieReaderProvider
    {
        public bool IsFileFormatSupported(string path)
        {
            if (Path.GetExtension(path) == ".json")
                return true;

            using var reader = new StreamReader(path);

            int chi;

            while ((chi = reader.Read()) != -1)
            {
                var ch = (char)chi;

                if (char.IsWhiteSpace(ch))
                    continue;

                return ch == '[';
            }

            return false;
        }

        public IEnumerable<Cookie> ReadAllCookies(string path, string domain = null, bool useExpirationTimestamp = false)
        {
            using var reader = new StreamReader(path);
            var json = Json.Parse(reader);

            foreach (var cookieJson in json.AsArray())
            {
                var cookieDomain = cookieJson["domain"].Get<string>();

                if (domain != null && !cookieDomain.EndsWith(domain))
                    continue;

                var cookieName = cookieJson["name"].Get<string>();

                if (string.IsNullOrEmpty(cookieName))
                    continue;

                var cookie = new Cookie()
                {
                    Domain = cookieDomain,
                    Path = cookieJson["path"].Get<string>(),
                    Secure = cookieJson["secure"].Get<bool>(),
                    Name = cookieName,
                    Value = cookieJson["value"].Get<string>()
                };

                if (useExpirationTimestamp)
                    cookie.Expires = TimeConverter.FromUnixTimestamp(cookieJson["expirationDate"].Get<double>());

                yield return cookie;
            }
        }
    }
}
