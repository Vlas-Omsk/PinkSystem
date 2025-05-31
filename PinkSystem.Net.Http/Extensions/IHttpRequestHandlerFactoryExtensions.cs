using Microsoft.Extensions.Logging;
using PinkSystem.Net.Http.Handlers;
using PinkSystem.Net.Http.Handlers.Pooling;
using System;

namespace PinkSystem.Net.Http
{
    public static class IHttpRequestHandlerFactoryExtensions
    {
        public static IHttpRequestHandlerFactory WithRepeating(this IHttpRequestHandlerFactory self, int retryAmount, TimeSpan retryDelay, ILoggerFactory logger)
        {
            return new RepeatHttpRequestHandlerFactory(self, retryAmount, retryDelay, logger);
        }

        public static IHttpRequestHandlerFactory WithStatisticsCollecting(this IHttpRequestHandlerFactory self, StatisticsStorage storage)
        {
            return new StatisticsHttpRequestHandlerFactory(self, storage);
        }

        public static IHttpRequestHandlerFactory WithNoPooling(this IHttpRequestHandlerFactory self)
        {
            return new NonPooledHttpRequestHandlerFactory(self);
        }
    }
}
