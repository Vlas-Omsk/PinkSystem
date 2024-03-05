using BotsCommon.Net.Http.Handlers;
using Microsoft.Extensions.Logging;
using System;

namespace BotsCommon.Net.Http
{
    public sealed class SystemNetHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        private readonly int _retryAmount;
        private readonly TimeSpan _retryDelay;
        private readonly ILoggerFactory _loggerFactory;

        public SystemNetHttpRequestHandlerFactory(
            int retryAmount,
            TimeSpan retryDelay,
            ILoggerFactory loggerFactory
        )
        {
            _retryAmount = retryAmount;
            _retryDelay = retryDelay;
            _loggerFactory = loggerFactory;
        }

        public IHttpRequestHandler Create(HttpRequestHandlerOptions options)
        {
            IHttpRequestHandler handler = new SystemNetHttpRequestHandler(options);

            handler = new SystemNetRepeatHttpRequestHandler(
                handler,
                _retryAmount,
                _retryDelay,
                _loggerFactory.CreateLogger<SystemNetRepeatHttpRequestHandler>()
            );

            handler = new CompressHttpRequestHandler(handler);

            return handler;
        }
    }
}
