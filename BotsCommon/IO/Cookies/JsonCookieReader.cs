using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            using var streamReader = new StreamReader(path);
            using var reader = new JsonTextReader(streamReader);

            var json = JToken.Load(reader);

            foreach (var cookieJson in json)
            {
                var cookieDomain = cookieJson["domain"].Value<string>();

                if (Domain != null && !cookieDomain.EndsWith(Domain))
                    continue;

                var cookieName = cookieJson["name"].Value<string>();

                if (string.IsNullOrEmpty(cookieName))
                    continue;

                var cookie = new Cookie()
                {
                    Domain = cookieDomain,
                    Path = cookieJson["path"].Value<string>(),
                    Secure = cookieJson["secure"].Value<bool>(),
                    Name = cookieName,
                    Value = cookieJson["value"].Value<string>()
                };

                if (UseExpirationTimestamp)
                    cookie.Expires = UnixTimestamp.FromSeconds(cookieJson["expirationDate"].Value<double>()).DateTime;

                yield return cookie;
            }
        }
    }
}
