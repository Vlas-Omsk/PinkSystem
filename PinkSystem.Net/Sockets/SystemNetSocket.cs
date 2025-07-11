using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System;
using PinkSystem.Net.Interop;

namespace PinkSystem.Net.Sockets
{
    public sealed class SystemNetSocket : BaseSocket
    {
        public SystemNetSocket(Socket socket)
        {
            Socket = socket;
        }

        public Socket Socket { get; }

        public override bool NoDelay
        {
            get => Socket.NoDelay;
            set => Socket.NoDelay = value;
        }

        public override LingerOption LingerState
        {
            get => Socket.LingerState!;
            set => Socket.LingerState = value;
        }

        public override void Bind(EndPoint localEndPoint)
        {
            Socket.Bind(localEndPoint);
        }

        public override void BindToDevice(string interfaceName)
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

        public override ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken)
        {
            return Socket.ConnectAsync(endPoint, cancellationToken);
        }

        protected override Stream GetStreamOverride()
        {
            return new NetworkStream(Socket, ownsSocket: true);
        }

        public override void SetSocketOption(SocketOptionLevel level, SocketOptionName name, object value)
        {
            Socket.SetSocketOption(level, name, value);
        }

        public override void SetSocketOption(SocketOptionLevel level, SocketOptionName name, int value)
        {
            Socket.SetSocketOption(level, name, value);
        }

        public override void SetSocketOption(SocketOptionLevel level, SocketOptionName name, byte[] value)
        {
            Socket.SetSocketOption(level, name, value);
        }

        public override void SetSocketOption(SocketOptionLevel level, SocketOptionName name, bool value)
        {
            Socket.SetSocketOption(level, name, value);
        }

        protected override void DisposeOverride(bool streamDisposing)
        {
            if (!streamDisposing)
                Socket.Dispose();
        }
    }
}
