using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers
{
    public sealed class RedirectHttpRequestHandler : ExtensionHttpRequestHandler
    {
        public RedirectHttpRequestHandler(IHttpRequestHandler handler) : base(handler)
        {
        }

        public override async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            while (true)
            {
                var response = await Handler.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (response.StatusCode is
                    not HttpStatusCode.Moved and
                    not HttpStatusCode.Found and
                    not HttpStatusCode.TemporaryRedirect and
                    not HttpStatusCode.PermanentRedirect)
                    return response;

                if (!response.Headers.TryGetValues("Location", out var locationValues))
                    return response;

                var location = locationValues.Single();

                var nextRequest = new HttpRequest(
                    request.Method,
                    new Uri(response.Uri, location)
                )
                {
                    Content = request.Content
                };

                request.Headers.CopyTo(nextRequest.Headers);

                request = nextRequest;
            }
        }
    }
}
