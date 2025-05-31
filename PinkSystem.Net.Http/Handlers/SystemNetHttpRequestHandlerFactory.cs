using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using PinkSystem.Net.Sockets;

namespace PinkSystem.Net.Http.Handlers
{
    public sealed class SystemNetHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        private readonly ISocketsProvider _socketsProvider;

        private sealed class SystemNetHttpRequestHandler : IHttpRequestHandler
        {
            private readonly HttpClient _httpClient;

            public SystemNetHttpRequestHandler(ISocketsProvider socketsProvider, IHttpRequestHandlerOptions? options)
            {
                var handler = new SocketsHttpHandler()
                {
                    AutomaticDecompression = DecompressionMethods.None,
                    AllowAutoRedirect = false,
                    ConnectCallback = SystemNetHttpUtils.CreateConnectCallback(socketsProvider)
                };

                if (options != null)
                {
                    if (options.Proxy != null)
                        handler.Proxy = options.Proxy.ToWebProxy();
                }

                _httpClient = new HttpClient(handler)
                {
                    Timeout = options?.Timeout ?? HttpTimeout.Default
                };
            }

            public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
            {
                using var requestMessage = SystemNetHttpUtils.CreateNetRequestFromRequest(request);

                using var responseMessage = await _httpClient.SendWithExceptionWrappingAsync(requestMessage, cancellationToken).ConfigureAwait(false);

                return await SystemNetHttpUtils.CreateResponseFromNetResponse(responseMessage, cancellationToken).ConfigureAwait(false);
            }

            public void Dispose()
            {
                _httpClient.Dispose();
            }
        }

        public SystemNetHttpRequestHandlerFactory(ISocketsProvider socketsProvider)
        {
            _socketsProvider = socketsProvider;
        }

        public IHttpRequestHandler Create(IHttpRequestHandlerOptions? options)
        {
            return new SystemNetHttpRequestHandler(_socketsProvider, options);
        }
    }
}
