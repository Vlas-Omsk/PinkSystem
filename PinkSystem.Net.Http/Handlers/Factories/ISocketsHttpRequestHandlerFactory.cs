using PinkSystem.Net.Sockets;

namespace PinkSystem.Net.Http.Handlers.Factories
{
    public interface ISocketsHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        ISocketsProvider SocketsProvider { get; }
    }
}
