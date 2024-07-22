using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http.Sockets
{
    public interface ISocket : IDisposable
    {
        bool NoDelay { get; set; }
        LingerOption? LingerState { get; set; }
        
        void SetSocketOption(SocketOptionLevel level, SocketOptionName name, object value);
        void SetSocketOption(SocketOptionLevel level, SocketOptionName name, int value);
        void SetSocketOption(SocketOptionLevel level, SocketOptionName name, byte[] value);
        void SetSocketOption(SocketOptionLevel level, SocketOptionName name, bool value);
        ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken);
        Stream GetStream();
    }
}
