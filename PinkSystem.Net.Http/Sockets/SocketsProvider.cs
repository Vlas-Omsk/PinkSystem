using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Sockets
{
    public sealed class SocketsProvider : ISocketsProvider
    {
        public int MaxAvailableSockets { get; } = int.MaxValue;
        public int CurrentAvailableSockets { get; } = int.MaxValue;
        public bool NoDelay { get; set; } = true;
        public LingerOption? LingerState { get; set; }

        public Task<ISocket> Create(SocketType socketType, ProtocolType protocolType, CancellationToken cancellationToken)
        {
            var socket = new DefaultSocket(new(socketType, protocolType))
            {
                NoDelay = NoDelay
            };

            if (LingerState != null)
                socket.LingerState = LingerState;

            return Task.FromResult<ISocket>(socket);
        }
    }
}
