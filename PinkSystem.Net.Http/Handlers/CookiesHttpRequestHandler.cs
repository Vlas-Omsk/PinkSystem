using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers
{
    public sealed class CookiesHttpRequestHandler : ExtensionHttpRequestHandler
    {
        private readonly CookieContainer _cookieContainer;

        public CookiesHttpRequestHandler(IHttpRequestHandler handler, CookieContainer cookieContainer) : base(handler)
        {
            _cookieContainer = cookieContainer;
        }

        public override async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            var cookieHeaderValue = _cookieContainer.GetCookieHeader(request.Uri);

            if (!string.IsNullOrEmpty(cookieHeaderValue))
                request.Headers.Add("Cookie", cookieHeaderValue);

            var response = await Handler.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
                foreach (var setCookieHeader in setCookieHeaders)
                    _cookieContainer.SetCookies(response.Uri, setCookieHeader);

            return response;
        }
    }
}
