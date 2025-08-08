using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Sockets
{
    public abstract class ExtensionSocket : ISocket
    {
        private readonly ISocket _socket;

        protected ExtensionSocket(ISocket socket)
        {
            _socket = socket;
        }

        public virtual int Available => _socket.Available;

        public virtual bool Blocking
        {
            get => _socket.Blocking;
            set => _socket.Blocking = value;
        }

        public virtual bool DontFragment
        {
            get => _socket.DontFragment;
            set => _socket.DontFragment = value;
        }

        public virtual bool DualMode
        {
            get => _socket.DualMode;
            set => _socket.DualMode = value;
        }

        public virtual bool EnableBroadcast
        {
            get => _socket.EnableBroadcast;
            set => _socket.EnableBroadcast = value;
        }

        public virtual bool NoDelay
        {
            get => _socket.NoDelay;
            set => _socket.NoDelay = value;
        }

        public virtual LingerOption LingerState
        {
            get => _socket.LingerState;
            set => _socket.LingerState = value;
        }

        public virtual bool Connected => _socket.Connected;
        public virtual bool ExclusiveAddressUse => _socket.ExclusiveAddressUse;
        public virtual bool IsBound => _socket.IsBound;

        public virtual bool MulticastLoopback
        {
            get => _socket.MulticastLoopback;
            set => _socket.MulticastLoopback = value;
        }

        public virtual short Ttl
        {
            get => _socket.Ttl;
            set => _socket.Ttl = value;
        }

        public virtual void SetSocketOption(SocketOptionLevel level, SocketOptionName name, object value)
        {
            _socket.SetSocketOption(level, name, value);
        }

        public virtual object? GetSocketOption(SocketOptionLevel level, SocketOptionName name)
        {
            return _socket.GetSocketOption(level, name);
        }

        public virtual void Bind(EndPoint localEndPoint)
        {
            _socket.Bind(localEndPoint);
        }

        public virtual void BindToDevice(string interfaceName)
        {
            _socket.BindToDevice(interfaceName);
        }

        public virtual ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken)
        {
            return _socket.ConnectAsync(endPoint, cancellationToken);
        }

        public virtual ValueTask DisconnectAsync(bool reuseSocket, CancellationToken cancellationToken)
        {
            return _socket.DisconnectAsync(reuseSocket, cancellationToken);
        }

        public virtual ValueTask<ISocket> AcceptAsync(CancellationToken cancellationToken)
        {
            return _socket.AcceptAsync(cancellationToken);
        }

        public virtual void Listen()
        {
            _socket.Listen();
        }

        public virtual void Shutdown(SocketShutdown how)
        {
            _socket.Shutdown(how);
        }

        public virtual void Close(TimeSpan timeout)
        {
            _socket.Close(timeout);
        }

        public virtual bool Poll(TimeSpan timeout, SelectMode mode)
        {
            return _socket.Poll(timeout, mode);
        }

        public virtual ValueTask<int> ReceiveAsync(Memory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            return _socket.ReceiveAsync(buffer, socketFlags, cancellationToken);
        }

        public virtual ValueTask<SocketReceiveFromResult> ReceiveFromAsync(Memory<byte> buffer, SocketFlags socketFlags, EndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            return _socket.ReceiveFromAsync(buffer, socketFlags, remoteEndPoint, cancellationToken);
        }

        public virtual ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            return _socket.SendAsync(buffer, socketFlags, cancellationToken);
        }

        public virtual ValueTask<int> SendToAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, EndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            return _socket.SendToAsync(buffer, socketFlags, remoteEndPoint, cancellationToken);
        }

        public virtual void Dispose()
        {
            _socket.Dispose();
        }
    }
}
