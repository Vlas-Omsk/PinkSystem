using BotsCommon.Net.Http.Sockets;

namespace BotsCommon.Net.Http
{
    public sealed record SystemNetSocketOptions
    {
        public ISocketsProvider Provider { get; init; } = new SocketsProvider();
        public bool DisableLingering { get; init; }
    }
}
