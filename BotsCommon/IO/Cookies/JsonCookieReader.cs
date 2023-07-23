using PinkJson2;
using System.Net;

namespace BotsCommon.IO.Cookies
{
    public sealed class JsonCookieReaderProvider : ICookieReader
    {
        public string Domain { get; set; }
        public bool UseExpirationTimestamp { get; set; }

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

        public IEnumerable<Cookie> ReadAllCookies(string path)
        {
            using var reader = new StreamReader(path);
            var json = Json.Parse(reader);

            foreach (var cookieJson in json.AsArray())
            {
                var cookieDomain = cookieJson["domain"].Get<string>();

                if (Domain != null && !cookieDomain.EndsWith(Domain))
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

                if (UseExpirationTimestamp)
                    cookie.Expires = new UnixTimestamp(cookieJson["expirationDate"].Get<double>()).DateTime;

                yield return cookie;
            }
        }
    }
}
