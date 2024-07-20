using BotsCommon.Net.Http.Sockets;

namespace BotsCommon.Net.Http.Handlers.Factories
{
    public interface ISocketsHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        ISocketsProvider SocketsProvider { get; }
    }
}
