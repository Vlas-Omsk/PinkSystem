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

        public SystemNetHttpRequestHandler(HttpRequestHandlerOptions options, SystemNetSocketOptions socketOptions, TimeSpan timeout)
        {
            var handler = new SocketsHttpHandler()
            {
                AutomaticDecompression = DecompressionMethods.None,
                AllowAutoRedirect = false
            };

            socketOptions.Apply(handler);

            if (options.Proxy != null)
                handler.Proxy = options.Proxy.ToWebProxy();

            if (!options.ValidateSsl)
                handler.SslOptions = new()
                {
                    RemoteCertificateValidationCallback = delegate { return true; }
                };

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

            using var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            return await SystemNetHttpUtils.CreateResponseFromNetResponse(responseMessage, cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
