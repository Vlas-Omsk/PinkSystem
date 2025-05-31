using System.Net;
using PinkSystem.Net.Http.Handlers;

namespace PinkSystem.Net.Http
{
    public static class IHttpRequestHandlerExtensions
    {
        public static IHttpRequestHandler WithCompression(this IHttpRequestHandler self)
        {
            return new CompressHttpRequestHandler(self);
        }

        public static IHttpRequestHandler WithConcurrency(this IHttpRequestHandler self, int concurrency)
        {
            return new ConcurrentHttpRequestHandler(self, concurrency);
        }

        public static IHttpRequestHandler WithCookies(this IHttpRequestHandler self, CookieContainer cookieContainer)
        {
            return new CookiesHttpRequestHandler(self, cookieContainer);
        }

        public static IHttpRequestHandler WithRedirectionFollowing(this IHttpRequestHandler self)
        {
            return new RedirectHttpRequestHandler(self);
        }
    }
}
