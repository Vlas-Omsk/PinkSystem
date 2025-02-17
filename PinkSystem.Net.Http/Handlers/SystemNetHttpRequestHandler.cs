using PinkSystem.Net.Sockets;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers
{
    public sealed class SystemNetHttpRequestHandler : IHttpRequestHandler
    {
        private readonly HttpClient _httpClient;
        private readonly ISocketsProvider _socketsProvider;
        private readonly TimeSpan _timeout;

        public SystemNetHttpRequestHandler(HttpRequestHandlerOptions options, ISocketsProvider socketsProvider, TimeSpan timeout)
        {
            var handler = new SocketsHttpHandler()
            {
                AutomaticDecompression = DecompressionMethods.None,
                AllowAutoRedirect = false,
                ConnectCallback = SystemNetHttpUtils.CreateConnectCallback(socketsProvider)
            };

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

            _socketsProvider = socketsProvider;
            _timeout = timeout;

            Options = options;
        }
        
        public HttpRequestHandlerOptions Options { get; }

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            using var requestMessage = SystemNetHttpUtils.CreateNetRequestFromRequest(request);

            using var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            return await SystemNetHttpUtils.CreateResponseFromNetResponse(responseMessage, cancellationToken).ConfigureAwait(false);
        }

        public IHttpRequestHandler Clone()
        {
            return new SystemNetHttpRequestHandler(Options, _socketsProvider, _timeout);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
