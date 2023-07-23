using System.Net;

namespace BotsCommon
{
    public static class IEnumerableCookieExtensions
    {
        public static CookieContainer ToCookieContainer(this IEnumerable<Cookie> self)
        {
            var cookieContainer = new CookieContainer();

            foreach (var cookie in self)
                cookieContainer.Add(cookie);

            return cookieContainer;
        }

        public static CookieContainer ToCookieContainer(this IEnumerable<Cookie> self, Uri uri)
        {
            var cookieContainer = new CookieContainer();

            foreach (var cookie in self)
                cookieContainer.Add(uri, cookie);

            return cookieContainer;
        }
    }
}
