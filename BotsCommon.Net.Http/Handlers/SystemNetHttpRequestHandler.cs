using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http.Handlers
{
    public sealed class SystemNetHttpRequestHandler : IHttpRequestHandler
    {
        private readonly HttpClient _httpClient;

        public SystemNetHttpRequestHandler(HttpRequestHandlerOptions options, TimeSpan timeout)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.None,
                AllowAutoRedirect = false
            };

            if (options.Proxy != null)
                handler.Proxy = options.Proxy.ToWebProxy();

            _httpClient = new HttpClient(handler)
            {
                Timeout = timeout
            };

            Options = options;
        }

        public HttpRequestHandlerOptions Options { get; }

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            using var requestMessage = SystemNetHttpUtils.CreateNetRequestFromRequest(request);

            var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            return await SystemNetHttpUtils.CreateResponseFromNetResponse(responseMessage, cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
