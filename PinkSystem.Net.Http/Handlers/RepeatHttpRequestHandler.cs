using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers
{
    public sealed class RepeatHttpRequestHandler : IHttpRequestHandler
    {
        private readonly IHttpRequestHandler _handler;
        private readonly int _retryAmount;
        private readonly TimeSpan _retryDelay;
        private readonly ILogger<RepeatHttpRequestHandler> _logger;

        public RepeatHttpRequestHandler(
            IHttpRequestHandler handler,
            int retryAmount,
            TimeSpan retryDelay,
            ILogger<RepeatHttpRequestHandler> logger
        )
        {
            _handler = handler;
            _retryAmount = retryAmount;
            _retryDelay = retryDelay;
            _logger = logger;
        }

        public HttpRequestHandlerOptions Options => _handler.Options;

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            Exception? exLast = null;

            for (var i = 1; i <= _retryAmount; i++)
            {
                try
                {
                    return await _handler.SendAsync(request, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is ProxyConnectionRefusedException or HttpConnectionRefusedException)
                {
                    exLast = ex;

                    _logger.LogWarning(ex, "Error when sending request {method} {uri}. Retry {i}/{maxRetryCount} after {delay}", request.Method, request.Uri, i, _retryAmount, _retryDelay);
                }

                if (i < _retryAmount)
                    await Task.Delay(_retryDelay, cancellationToken).ConfigureAwait(false);
            }

            throw new Exception("The number of attempts has been exhausted", exLast!);
        }

        public IHttpRequestHandler Clone()
        {
            return new RepeatHttpRequestHandler(_handler.Clone(), _retryAmount, _retryDelay, _logger);
        }

        public void Dispose()
        {
            _handler.Dispose();
        }
    }
}
