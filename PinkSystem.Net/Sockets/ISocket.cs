using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Sockets
{
    public interface ISocket : IDisposable
    {
        int Available { get; }
        bool Blocking { get; set; }
        bool DontFragment { get; set; }
        bool DualMode { get; set; }
        bool EnableBroadcast { get; set; }
        bool NoDelay { get; set; }
        LingerOption LingerState { get; set; }
        bool Connected { get; }
        bool ExclusiveAddressUse { get; }
        bool IsBound { get; }
        bool MulticastLoopback { get; set; }
        short Ttl { get; set; }

        void SetSocketOption(SocketOptionLevel level, SocketOptionName name, object value);
        object? GetSocketOption(SocketOptionLevel level, SocketOptionName name);
        void Bind(EndPoint localEndPoint);
        void BindToDevice(string interfaceName);
        ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken);
        ValueTask DisconnectAsync(bool reuseSocket, CancellationToken cancellationToken);
        ValueTask<ISocket> AcceptAsync(CancellationToken cancellationToken);
        void Listen();
        void Shutdown(SocketShutdown how);
        void Close(TimeSpan timeout);
        bool Poll(TimeSpan timeout, SelectMode mode);
        ValueTask<int> ReceiveAsync(Memory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken);
        ValueTask<SocketReceiveFromResult> ReceiveFromAsync(Memory<byte> buffer, SocketFlags socketFlags, EndPoint remoteEndPoint, CancellationToken cancellationToken);
        ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken);
        ValueTask<int> SendToAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, EndPoint remoteEndPoint, CancellationToken cancellationToken);
    }
}
