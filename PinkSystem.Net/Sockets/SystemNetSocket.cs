using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System;
using PinkSystem.Net.Interop;

namespace PinkSystem.Net.Sockets
{
    public class SystemNetSocket : ISocket
    {
        private sealed class StreamWrapper : Stream
        {
            private readonly Stream _stream;
            private readonly SystemNetSocket _socket;

            public StreamWrapper(Stream stream, SystemNetSocket socket)
            {
                _stream = stream;
                _socket = socket;
            }

            public override bool CanRead => _stream.CanRead;
            public override bool CanSeek => _stream.CanSeek;
            public override bool CanTimeout => _stream.CanTimeout;
            public override bool CanWrite => _stream.CanWrite;
            public override long Length => _stream.Length;

            public override int ReadTimeout
            {
                get => _stream.ReadTimeout;
                set => _stream.ReadTimeout = value;
            }

            public override int WriteTimeout
            {
                get => _stream.WriteTimeout;
                set => _stream.WriteTimeout = value;
            }

            public override long Position
            {
                get => _stream.Position;
                set => _stream.Position = value;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                var amount = _stream.Read(buffer, offset, count);

                return amount;
            }

            public override int Read(Span<byte> buffer)
            {
                var amount = _stream.Read(buffer);

                return amount;
            }

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                var amount = _stream.ReadAsync(buffer, offset, count, cancellationToken);

                return amount;
            }

            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                var amount = _stream.ReadAsync(buffer, cancellationToken);

                return amount;
            }

            public override int ReadByte()
            {
                var result = _stream.ReadByte();

                return result;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _stream.Write(buffer, offset, count);
            }

            public override void Write(ReadOnlySpan<byte> buffer)
            {
                _stream.Write(buffer);
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return _stream.WriteAsync(buffer, offset, count, cancellationToken);
            }

            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                return _stream.WriteAsync(buffer, cancellationToken);
            }

            public override void WriteByte(byte value)
            {
                _stream.WriteByte(value);
            }

            public override void Close()
            {
                _stream.Close();

                base.Close();
            }

            public override void CopyTo(Stream destination, int bufferSize)
            {
                _stream.CopyTo(destination, bufferSize);
            }

            public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            {
                return _stream.CopyToAsync(destination, bufferSize, cancellationToken);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _socket.Dispose(streamDisposing: true);

                    _stream.Dispose();
                }

                base.Dispose(disposing);
            }

            public override ValueTask DisposeAsync()
            {
                _socket.Dispose(streamDisposing: true);

                return _stream.DisposeAsync();
            }

            public override void Flush()
            {
                _stream.Flush();
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                return _stream.FlushAsync(cancellationToken);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _stream.SetLength(value);
            }

            public override string? ToString()
            {
                return _stream.ToString();
            }
        }

        public SystemNetSocket(Socket socket)
        {
            Socket = socket;
        }

        protected Socket Socket { get; }

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

        public virtual void Bind(EndPoint localEndPoint)
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

        public virtual ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken)
        {
            return Socket.ConnectAsync(endPoint, cancellationToken);
        }

        public virtual Stream GetStream()
        {
            return new StreamWrapper(
                new NetworkStream(Socket, ownsSocket: true),
                this
            );
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, object value)
        {
            Socket.SetSocketOption(level, name, value);
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, int value)
        {
            Socket.SetSocketOption(level, name, value);
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, byte[] value)
        {
            Socket.SetSocketOption(level, name, value);
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, bool value)
        {
            Socket.SetSocketOption(level, name, value);
        }

        public void Dispose()
        {
            Dispose(streamDisposing: false);
        }

        protected virtual void Dispose(bool streamDisposing)
        {
            if (!streamDisposing)
                Socket.Dispose();
        }
    }
}
