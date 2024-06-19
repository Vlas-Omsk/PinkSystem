using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http.Handlers
{
    public sealed class SystemNetNonPooledHttpRequestHandler : IHttpRequestHandler
    {
        private readonly SystemNetSocketOptions _socketOptions;
        private readonly TimeSpan _timeout;

        public SystemNetNonPooledHttpRequestHandler(HttpRequestHandlerOptions options, SystemNetSocketOptions socketOptions, TimeSpan timeout)
        {
            Options = options;
            _socketOptions = socketOptions;
            _timeout = timeout;
        }

        public HttpRequestHandlerOptions Options { get; }

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            using var requestMessage = SystemNetHttpUtils.CreateNetRequestFromRequest(request);

            var handler = new SocketsHttpHandler()
            {
                AutomaticDecompression = DecompressionMethods.None,
                AllowAutoRedirect = false
            };

            _socketOptions.Apply(handler);

            if (Options.Proxy != null)
                handler.Proxy = Options.Proxy.ToWebProxy();

            using var httpClient = new HttpClient(handler)
            {
                Timeout = _timeout,
            };

            using var responseMessage = await httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            return await SystemNetHttpUtils.CreateResponseFromNetResponse(responseMessage, cancellationToken).ConfigureAwait(false);
        }

        public IHttpRequestHandler Clone()
        {
            return new SystemNetNonPooledHttpRequestHandler(Options, _socketOptions, _timeout);
        }

        public void Dispose()
        {
        }
    }
}
