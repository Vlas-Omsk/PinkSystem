using PinkSystem.Net.Http.Sockets;
using System;

namespace PinkSystem.Net.Http.Handlers.Factories
{
    public sealed class SystemNetNonPooledHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        private readonly ISocketsProvider _socketsProvider;
        private readonly TimeSpan _timeout;

        public SystemNetNonPooledHttpRequestHandlerFactory(ISocketsProvider socketsProvider, TimeSpan timeout)
        {
            _socketsProvider = socketsProvider;
            _timeout = timeout;
        }

        public IHttpRequestHandler Create(HttpRequestHandlerOptions options)
        {
            return new SystemNetNonPooledHttpRequestHandler(options, _socketsProvider, _timeout);
        }
    }
}
