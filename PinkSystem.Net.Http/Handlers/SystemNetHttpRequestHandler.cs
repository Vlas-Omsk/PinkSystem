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
        private readonly SocketsHttpHandler _handler;
        private readonly HttpClient _httpClient;
        private readonly ISocketsProvider _socketsProvider;
        private Proxy? _proxy = null;
        private bool _validateSsl = true;
        private TimeSpan _timeout = HttpTimeout.Default;

        public SystemNetHttpRequestHandler(ISocketsProvider socketsProvider)
        {
            _handler = new SocketsHttpHandler()
            {
                AutomaticDecompression = DecompressionMethods.None,
                AllowAutoRedirect = false,
                ConnectCallback = SystemNetHttpUtils.CreateConnectCallback(socketsProvider)
            };
            _httpClient = new HttpClient(_handler)
            {
                Timeout = _timeout
            };

            _socketsProvider = socketsProvider;
        }

        public Proxy? Proxy
        {
            get => _proxy;
            set
            {
                _proxy = value;
                _handler.Proxy = value?.ToWebProxy();
            }
        }

        public bool ValidateSsl
        {
            get => _validateSsl;
            set
            {
                _validateSsl = value;

                if (_validateSsl)
                {
                    _handler.SslOptions.RemoteCertificateValidationCallback = null;
                }
                else
                {
                    _handler.SslOptions.RemoteCertificateValidationCallback = delegate { return true; };
                }
            }
        }

        public TimeSpan Timeout
        {
            get => _timeout;
            set
            {
                _timeout = value;
                _httpClient.Timeout = value;
            }
        }

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            using var requestMessage = SystemNetHttpUtils.CreateNetRequestFromRequest(request);

            using var responseMessage = await _httpClient.SendWithExceptionWrappingAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            return await SystemNetHttpUtils.CreateResponseFromNetResponse(responseMessage, cancellationToken).ConfigureAwait(false);
        }

        public IHttpRequestHandler Clone()
        {
            var handler = new SystemNetHttpRequestHandler(_socketsProvider);

            this.CopySettingsTo(handler);

            return handler;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
