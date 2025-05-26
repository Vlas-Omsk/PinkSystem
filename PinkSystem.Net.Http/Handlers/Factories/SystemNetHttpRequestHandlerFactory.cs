using PinkSystem.Net.Sockets;

namespace PinkSystem.Net.Http.Handlers.Factories
{
    public sealed class SystemNetHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        private readonly ISocketsProvider _socketsProvider;

        public SystemNetHttpRequestHandlerFactory(ISocketsProvider socketsProvider)
        {
            _socketsProvider = socketsProvider;
        }

        public IHttpRequestHandler Create()
        {
            return new SystemNetHttpRequestHandler(_socketsProvider);
        }
    }
}
