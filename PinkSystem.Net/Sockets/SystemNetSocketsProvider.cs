using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Sockets
{
    public sealed class SystemNetSocketsProvider : ISocketsProvider
    {
        public int MaxAvailableSockets { get; } = int.MaxValue;
        public int CurrentAvailableSockets { get; } = int.MaxValue;

        public Task<ISocket> Create(SocketType socketType, ProtocolType protocolType, CancellationToken cancellationToken)
        {
            var socket = new SystemNetSocket(new(socketType, protocolType));

            return Task.FromResult<ISocket>(socket);
        }
    }
}
