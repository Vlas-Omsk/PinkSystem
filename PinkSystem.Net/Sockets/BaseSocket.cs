using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Sockets
{
    public abstract class BaseSocket : ISocket
    {
        private sealed class StreamWrapper : Stream
        {
            private readonly Stream _stream;
            private readonly BaseSocket _socket;

            public StreamWrapper(Stream stream, BaseSocket socket)
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
                    _socket.DisposeOverride(streamDisposing: true);

                    _stream.Dispose();
                }

                base.Dispose(disposing);
            }

            public override ValueTask DisposeAsync()
            {
                _socket.DisposeOverride(streamDisposing: true);

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

        public abstract bool NoDelay { get; set; }
        public abstract LingerOption LingerState { get; set; }

        public abstract void Bind(EndPoint localEndPoint);

        public abstract void BindToDevice(string interfaceName);

        public abstract ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken);

        public Stream GetStream()
        {
            return new StreamWrapper(
                GetStreamOverride(),
                this
            );
        }

        protected abstract Stream GetStreamOverride();
        public abstract void SetSocketOption(SocketOptionLevel level, SocketOptionName name, object value);
        public abstract void SetSocketOption(SocketOptionLevel level, SocketOptionName name, int value);
        public abstract void SetSocketOption(SocketOptionLevel level, SocketOptionName name, byte[] value);
        public abstract void SetSocketOption(SocketOptionLevel level, SocketOptionName name, bool value);

        public void Dispose()
        {
            DisposeOverride(streamDisposing: false);
        }

        protected abstract void DisposeOverride(bool streamDisposing);
    }
}
