using Microsoft.Extensions.Logging;
using BotsCommon.Net.Http.Handlers;

namespace BotsCommon.Net.Http
{
    public sealed class WgetHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        private readonly int _retryAmount;
        private readonly ILoggerFactory _loggerFactory;

        public WgetHttpRequestHandlerFactory(int retryAmount, ILoggerFactory loggerFactory)
        {
            _retryAmount = retryAmount;
            _loggerFactory = loggerFactory;
        }

        public IHttpRequestHandler Create(HttpRequestHandlerOptions options)
        {
            return new WgetHttpRequestHandler(
                options,
                _retryAmount,
                _loggerFactory.CreateLogger<WgetHttpRequestHandler>()
            );
        }
    }
}
