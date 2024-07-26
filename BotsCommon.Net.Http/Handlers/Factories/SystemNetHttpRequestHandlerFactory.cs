using BotsCommon.Net.Http.Sockets;
using System;

namespace BotsCommon.Net.Http.Handlers.Factories
{
    public sealed class SystemNetHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        private readonly ISocketsProvider _socketsProvider;
        private readonly TimeSpan _timeout;

        public SystemNetHttpRequestHandlerFactory(ISocketsProvider socketsProvider, TimeSpan timeout)
        {
            _socketsProvider = socketsProvider;
            _timeout = timeout;
        }

        public IHttpRequestHandler Create(HttpRequestHandlerOptions options)
        {
            return new SystemNetHttpRequestHandler(options, _socketsProvider, _timeout);
        }
    }
}
