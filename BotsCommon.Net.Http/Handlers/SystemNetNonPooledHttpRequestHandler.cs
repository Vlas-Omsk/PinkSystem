using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http.Handlers
{
    public sealed class SystemNetNonPooledHttpRequestHandler : IHttpRequestHandler
    {
        public SystemNetNonPooledHttpRequestHandler(HttpRequestHandlerOptions options)
        {
            Options = options;
        }

        public HttpRequestHandlerOptions Options { get; }

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            using var requestMessage = SystemNetHttpUtils.CreateNetRequestFromRequest(request);

            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.None,
                AllowAutoRedirect = false
            };

            if (Options.Proxy != null)
                handler.Proxy = Options.Proxy.ToWebProxy();

            using var httpClient = new HttpClient(handler);

            using var responseMessage = await httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            return await SystemNetHttpUtils.CreateResponseFromNetResponse(responseMessage, cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
        }
    }
}
