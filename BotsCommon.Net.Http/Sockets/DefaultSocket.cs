using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace BotsCommon.Net.Http.Sockets
{
    public class DefaultSocket : ISocket
    {
        public DefaultSocket(Socket socket)
        {
            Socket = socket;
        }

        protected Socket Socket { get; }

        public bool NoDelay
        {
            get => Socket.NoDelay;
            set => Socket.NoDelay = value;
        }

        public LingerOption? LingerState
        {
            get => Socket.LingerState;
            set => Socket.LingerState = value!;
        }

        public virtual ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken)
        {
            return Socket.ConnectAsync(endPoint, cancellationToken);
        }

        public virtual Stream GetStream()
        {
            return new NetworkStream(Socket, ownsSocket: true);
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, object value)
        {
            Socket.SetSocketOption(level, name, value);
        }

        public virtual void Dispose()
        {
            Socket.Dispose();
        }
    }
}
