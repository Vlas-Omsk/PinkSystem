using System;
using Microsoft.Extensions.Logging;
using PinkSystem.Net.Http.Handlers;
using PinkSystem.Net.Http.Handlers.Pooling;
using PinkSystem.Net.Sockets;

namespace PinkSystem.Net.Http
{
    public static class IHttpRequestHandlerFactoryExtensions
    {
        public static IHttpRequestHandlerFactory WithRepeating(this IHttpRequestHandlerFactory self, int retryAmount, TimeSpan retryDelay, ILoggerFactory loggerFactory)
        {
            return new RepeatHttpRequestHandlerFactory(self, retryAmount, retryDelay, loggerFactory);
        }

        public static IHttpRequestHandlerFactory WithStatisticsCollecting(this IHttpRequestHandlerFactory self, HttpRequestHandlerStatisticsStorage storage)
        {
            return new StatisticsHttpRequestHandlerFactory(self, storage);
        }

        public static IHttpRequestHandlerFactory WithNoPooling(this IHttpRequestHandlerFactory self)
        {
            return new NonPooledHttpRequestHandlerFactory(self);
        }

        public static IHttpRequestHandlerFactory WithPooling(this IHttpRequestHandlerFactory self, ISocketsProvider socketsProvider, ILoggerFactory loggerFactory)
        {
            var pool = new Pool(
                new PoolConnections(
                    self,
                    socketsProvider,
                    loggerFactory.CreateLogger<PoolConnections>()
                )
            );

            return new PooledHttpRequestHandlerFactory(pool);
        }

        public static IHttpRequestHandlerFactory WithSharedPooling(this IHttpRequestHandlerFactory self, ISocketsProvider socketsProvider, ILoggerFactory loggerFactory)
        {
            var pool = new SharedPool(
                new PoolConnections(
                    self,
                    socketsProvider,
                    loggerFactory.CreateLogger<PoolConnections>()
                )
            );

            return new PooledHttpRequestHandlerFactory(pool);
        }
    }
}
