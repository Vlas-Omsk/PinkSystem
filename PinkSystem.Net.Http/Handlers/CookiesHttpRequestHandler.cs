using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers
{
    public sealed class CookiesHttpRequestHandler : IHttpRequestHandler
    {
        private readonly IHttpRequestHandler _handler;
        private readonly CookieContainer _cookieContainer;

        public CookiesHttpRequestHandler(IHttpRequestHandler handler, CookieContainer cookieContainer)
        {
            _handler = handler;
            _cookieContainer = cookieContainer;
        }

        public HttpRequestHandlerOptions Options => _handler.Options;

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            var cookieHeaderValue = _cookieContainer.GetCookieHeader(request.Uri);

            if (!string.IsNullOrEmpty(cookieHeaderValue))
                request.Headers.Add("Cookie", cookieHeaderValue);

            var response = await _handler.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
                foreach (var setCookieHeader in setCookieHeaders)
                    _cookieContainer.SetCookies(response.Uri, setCookieHeader);

            return response;
        }

        public IHttpRequestHandler Clone()
        {
            return new CookiesHttpRequestHandler(_handler.Clone(), _cookieContainer);
        }

        public void Dispose()
        {
            _handler.Dispose();
        }
    }
}
