using BotsCommon.Net.Http.Sockets;

namespace BotsCommon.Net.Http.Handlers
{
    public sealed record SystemNetSocketOptions
    {
        public ISocketsProvider Provider { get; init; } = new SocketsProvider();
        public bool DisableLingering { get; init; }
    }
}
