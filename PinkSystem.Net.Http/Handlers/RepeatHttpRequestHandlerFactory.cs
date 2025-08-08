using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PinkSystem.Net.Http.Handlers
{
    public sealed class RepeatHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        private readonly IHttpRequestHandlerFactory _handlerFactory;
        private readonly int _retryAmount;
        private readonly TimeSpan _retryDelay;
        private readonly ILoggerFactory _loggerFactory;

        private sealed class RepeatHttpRequestHandler : ExtensionHttpRequestHandler
        {
            private readonly int _retryAmount;
            private readonly TimeSpan _retryDelay;
            private readonly ILogger<RepeatHttpRequestHandler> _logger;

            public RepeatHttpRequestHandler(
                IHttpRequestHandler handler,
                int retryAmount,
                TimeSpan retryDelay,
                ILogger<RepeatHttpRequestHandler> logger
            ) : base(handler)
            {
                _retryAmount = retryAmount;
                _retryDelay = retryDelay;
                _logger = logger;
            }

            public override async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
            {
                Exception? exLast = null;

                for (var i = 1; i <= _retryAmount; i++)
                {
                    try
                    {
                        return await Handler.SendAsync(request, cancellationToken).ConfigureAwait(false);
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
        }

        public RepeatHttpRequestHandlerFactory(
            IHttpRequestHandlerFactory httpRequestHandlerFactory,
            int retryAmount,
            TimeSpan retryDelay,
            ILoggerFactory loggerFactory
        )
        {
            _handlerFactory = httpRequestHandlerFactory;
            _retryAmount = retryAmount;
            _retryDelay = retryDelay;
            _loggerFactory = loggerFactory;
        }

        public IHttpRequestHandler Create(IHttpRequestHandlerOptions? options)
        {
            return new RepeatHttpRequestHandler(
                _handlerFactory.Create(options),
                _retryAmount,
                _retryDelay,
                _loggerFactory.CreateLogger<RepeatHttpRequestHandler>()
            );
        }
    }
}
