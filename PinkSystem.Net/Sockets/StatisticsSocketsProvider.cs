using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Sockets
{
    public sealed class StatisticsSocketsProvider : ISocketsProvider
    {
        private readonly ISocketsProvider _provider;
        private long _readBytes;
        private long _writeBytes;

        private sealed class StatisticsStream : Stream
        {
            private readonly Stream _stream;
            private readonly StatisticsSocketsProvider _provider;

            public StatisticsStream(Stream stream, StatisticsSocketsProvider provider)
            {
                _stream = stream;
                _provider = provider;
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

                AddReadBytes(amount);

                return amount;
            }

            public override int Read(Span<byte> buffer)
            {
                var amount = _stream.Read(buffer);

                AddReadBytes(amount);

                return amount;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                var amount = await _stream.ReadAsync(buffer, offset, count, cancellationToken);

                AddReadBytes(amount);

                return amount;
            }

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                var amount = await _stream.ReadAsync(buffer, cancellationToken);

                AddReadBytes(amount);

                return amount;
            }

            public override int ReadByte()
            {
                var result = _stream.ReadByte();

                if (result >= 0)
                {
                    unchecked
                    {
                        Interlocked.Increment(ref _provider._readBytes);
                    }
                }

                return result;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                AddWriteBytes(count);

                _stream.Write(buffer, offset, count);
            }

            public override void Write(ReadOnlySpan<byte> buffer)
            {
                AddWriteBytes(buffer.Length);

                _stream.Write(buffer);
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                AddWriteBytes(count);

                return _stream.WriteAsync(buffer, offset, count, cancellationToken);
            }

            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                AddWriteBytes(buffer.Length);

                return _stream.WriteAsync(buffer, cancellationToken);
            }

            public override void WriteByte(byte value)
            {
                unchecked
                {
                    Interlocked.Increment(ref _provider._writeBytes);
                }

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
                    _stream.Dispose();

                base.Dispose(disposing);
            }

            public override ValueTask DisposeAsync()
            {
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

            private void AddReadBytes(int amount)
            {
                unchecked
                {
                    Interlocked.Add(ref _provider._readBytes, amount);
                }
            }

            private void AddWriteBytes(int amount)
            {
                unchecked
                {
                    Interlocked.Add(ref _provider._writeBytes, amount);
                }
            }
        }

        private sealed class StatisticsSocket : ISocket
        {
            private readonly ISocket _socket;
            private readonly StatisticsSocketsProvider _provider;

            public StatisticsSocket(ISocket socket, StatisticsSocketsProvider provider)
            {
                _socket = socket;
                _provider = provider;
            }

            public bool NoDelay
            {
                get => _socket.NoDelay;
                set => _socket.NoDelay = value;
            }

            public LingerOption LingerState
            {
                get => _socket.LingerState;
                set => _socket.LingerState = value;
            }

            public ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken)
            {
                return _socket.ConnectAsync(endPoint, cancellationToken);
            }

            public Stream GetStream()
            {
                return new StatisticsStream(
                    _socket.GetStream(),
                    _provider
                );
            }

            public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, object value)
            {
                _socket.SetSocketOption(level, name, value);
            }

            public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, int value)
            {
                _socket.SetSocketOption(level, name, value);
            }

            public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, byte[] value)
            {
                _socket.SetSocketOption(level, name, value);
            }

            public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, bool value)
            {
                _socket.SetSocketOption(level, name, value);
            }

            public void Dispose()
            {
                _socket.Dispose();
            }
        }

        public StatisticsSocketsProvider(ISocketsProvider provider)
        {
            _provider = provider;
        }

        public int MaxAvailableSockets => _provider.MaxAvailableSockets;
        public int CurrentAvailableSockets => _provider.CurrentAvailableSockets;
        public long ReadBytes => _readBytes;
        public long WriteBytes => _writeBytes;

        public async Task<ISocket> Create(SocketType socketType, ProtocolType protocolType, CancellationToken cancellationToken)
        {
            return new StatisticsSocket(
                await _provider.Create(socketType, protocolType, cancellationToken),
                this
            );
        }
    }
}
