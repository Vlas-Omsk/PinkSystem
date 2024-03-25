using BotsCommon.IO.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace BotsCommon.Net.Http.Cookies
{
    public sealed class JsonCookieProvider : ICookieProvider
    {
        public string? Domain { get; set; }
        public bool UseExpirationTimestamp { get; set; }

        public bool IsSupported(IContentReader reader)
        {
            using var streamReader = new StreamReader(reader.CreateStream());

            int chi;

            while ((chi = streamReader.Read()) != -1)
            {
                var ch = (char)chi;

                if (char.IsWhiteSpace(ch))
                    continue;

                return ch == '[';
            }

            return false;
        }

        public IEnumerable<Cookie> ReadAll(IContentReader reader)
        {
            using var jsonReader = new JsonTextReader(new StreamReader(reader.CreateStream()));

            var json = JToken.Load(jsonReader);

            foreach (var cookieJson in json)
            {
                var cookieDomain = cookieJson.SelectTokenRequired("domain").ValueRequired<string>();

                if (Domain != null && !cookieDomain.EndsWith(Domain))
                    continue;

                var cookieName = cookieJson.SelectTokenRequired("name").Value<string>();

                if (string.IsNullOrEmpty(cookieName))
                    continue;

                var cookie = new Cookie()
                {
                    Domain = cookieDomain,
                    Path = cookieJson.SelectTokenRequired("path").Value<string>(),
                    Secure = cookieJson.SelectTokenRequired("secure").Value<bool>(),
                    Name = cookieName,
                    Value = cookieJson.SelectTokenRequired("value").Value<string>()
                };

                if (UseExpirationTimestamp)
                    cookie.Expires = UnixTimestamp.FromSeconds(
                        cookieJson.SelectTokenRequired("expirationDate").ValueRequired<double>()
                    ).DateTime;

                yield return cookie;
            }
        }
    }
}
