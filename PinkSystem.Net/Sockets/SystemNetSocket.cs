using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PinkSystem.Net.Interop;

namespace PinkSystem.Net.Sockets
{
    public sealed class SystemNetSocket : ISocket
    {
        public SystemNetSocket(Socket socket)
        {
            Socket = socket;
        }

        public Socket Socket { get; }

        public int Available => Socket.Available;

        public bool Blocking
        {
            get => Socket.Blocking;
            set => Socket.Blocking = value;
        }

        public bool DontFragment
        {
            get => Socket.DontFragment;
            set => Socket.DontFragment = value;
        }

        public bool DualMode
        {
            get => Socket.DualMode;
            set => Socket.DualMode = value;
        }

        public bool EnableBroadcast
        {
            get => Socket.EnableBroadcast;
            set => Socket.EnableBroadcast = value;
        }

        public bool NoDelay
        {
            get => Socket.NoDelay;
            set => Socket.NoDelay = value;
        }

        public LingerOption LingerState
        {
            get => Socket.LingerState!;
            set => Socket.LingerState = value;
        }

        public bool Connected => Socket.Connected;
        public bool ExclusiveAddressUse => Socket.ExclusiveAddressUse;
        public bool IsBound => Socket.IsBound;

        public bool MulticastLoopback
        {
            get => Socket.MulticastLoopback;
            set => Socket.MulticastLoopback = value;
        }

        public short Ttl
        {
            get => Socket.Ttl;
            set => Socket.Ttl = value;
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, object value)
        {
            if (value is int intValue)
                Socket.SetSocketOption(level, name, intValue);
            else if (value is byte[] bytesValue)
                Socket.SetSocketOption(level, name, bytesValue);
            else if (value is bool boolValue)
                Socket.SetSocketOption(level, name, boolValue);
            else
                Socket.SetSocketOption(level, name, value);
        }

        public object? GetSocketOption(SocketOptionLevel level, SocketOptionName name)
        {
            return Socket.GetSocketOption(level, name);
        }

        public void Bind(EndPoint localEndPoint)
        {
            Socket.Bind(localEndPoint);
        }

        public void BindToDevice(string interfaceName)
        {
            if (OperatingSystem.IsLinux())
            {
                Linux.BindSocketToDevice(Socket, interfaceName);
            }
            else
            {
                throw new PlatformNotSupportedException("Device binding not supported on Windows");
            }
        }

        public ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken)
        {
            return Socket.ConnectAsync(endPoint, cancellationToken);
        }

        public async ValueTask DisconnectAsync(bool reuseSocket, CancellationToken cancellationToken)
        {
            await Socket.DisconnectAsync(reuseSocket, cancellationToken);
        }

        public async ValueTask<ISocket> AcceptAsync(CancellationToken cancellationToken)
        {
            return new SystemNetSocket(
                await Socket.AcceptAsync(cancellationToken)
            );
        }

        public void Listen()
        {
            Socket.Listen();
        }

        public void Shutdown(SocketShutdown how)
        {
            Socket.Shutdown(how);
        }

        public void Close(TimeSpan timeout)
        {
            Socket.Close((int)timeout.TotalMilliseconds);
        }

        public bool Poll(TimeSpan timeout, SelectMode mode)
        {
            return Socket.Poll(timeout, mode);
        }

        public ValueTask<int> ReceiveAsync(Memory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            return Socket.ReceiveAsync(buffer, socketFlags, cancellationToken);
        }

        public ValueTask<SocketReceiveFromResult> ReceiveFromAsync(Memory<byte> buffer, SocketFlags socketFlags, EndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            return Socket.ReceiveFromAsync(buffer, socketFlags, remoteEndPoint, cancellationToken);
        }

        public ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            return Socket.SendAsync(buffer, socketFlags, cancellationToken);
        }

        public ValueTask<int> SendToAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, EndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            return Socket.SendToAsync(buffer, socketFlags, remoteEndPoint, cancellationToken);
        }

        public void Dispose()
        {
            Socket.Dispose();
        }
    }
}
