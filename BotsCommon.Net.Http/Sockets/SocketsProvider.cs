using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http.Sockets
{
    public sealed class SocketsProvider : ISocketsProvider
    {
        public int MaxAvailableSockets { get; } = int.MaxValue;
        public int CurrentAvailableSockets { get; } = int.MaxValue;
        public bool NoDelay { get; set; } = true;
        public LingerOption? LingerState { get; set; }

        public Task<ISocket> Create(SocketType socketType, ProtocolType protocolType, CancellationToken cancellationToken)
        {
            return Task.FromResult<ISocket>(new DefaultSocket(new(socketType, protocolType))
            {
                LingerState = LingerState,
                NoDelay = NoDelay
            });
        }
    }
}
