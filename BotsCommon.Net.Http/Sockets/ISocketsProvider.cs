using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http.Sockets
{
    public interface ISocketsProvider
    {
        int MaxAvailableSockets { get; }
        int CurrentAvailableSockets { get; }

        Task<ISocket> Create(SocketType socketType, ProtocolType protocolType, CancellationToken cancellationToken);
    }
}
