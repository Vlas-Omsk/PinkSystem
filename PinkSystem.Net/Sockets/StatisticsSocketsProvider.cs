using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Sockets
{
    public sealed class SocketStatisticsStorage
    {
        private long _readBytes;
        private long _writeBytes;

        public long ReadBytes => _readBytes;
        public long WriteBytes => _writeBytes;

        public void AddReadBytes(int amount)
        {
            unchecked
            {
                Interlocked.Add(ref _readBytes, amount);
            }
        }

        public void AddReadByte()
        {
            unchecked
            {
                Interlocked.Increment(ref _readBytes);
            }
        }

        public void AddWriteBytes(int amount)
        {
            unchecked
            {
                Interlocked.Add(ref _writeBytes, amount);
            }
        }

        public void AddWriteByte()
        {
            unchecked
            {
                Interlocked.Increment(ref _writeBytes);
            }
        }
    }

    public sealed class StatisticsSocketsProvider : ISocketsProvider
    {
        private readonly ISocketsProvider _provider;
        private readonly SocketStatisticsStorage _storage;

        private sealed class StatisticsStream : Stream
        {
            private readonly Stream _stream;
            private readonly SocketStatisticsStorage _storage;

            public StatisticsStream(Stream stream, SocketStatisticsStorage storage)
            {
                _stream = stream;
                _storage = storage;
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

                _storage.AddReadBytes(amount);

                return amount;
            }

            public override int Read(Span<byte> buffer)
            {
                var amount = _stream.Read(buffer);

                _storage.AddReadBytes(amount);

                return amount;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                var amount = await _stream.ReadAsync(buffer, offset, count, cancellationToken);

                _storage.AddReadBytes(amount);

                return amount;
            }

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                var amount = await _stream.ReadAsync(buffer, cancellationToken);

                _storage.AddReadBytes(amount);

                return amount;
            }

            public override int ReadByte()
            {
                var result = _stream.ReadByte();

                if (result >= 0)
                    _storage.AddReadByte();

                return result;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _storage.AddWriteBytes(count);

                _stream.Write(buffer, offset, count);
            }

            public override void Write(ReadOnlySpan<byte> buffer)
            {
                _storage.AddWriteBytes(buffer.Length);

                _stream.Write(buffer);
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                _storage.AddWriteBytes(count);

                return _stream.WriteAsync(buffer, offset, count, cancellationToken);
            }

            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                _storage.AddWriteBytes(buffer.Length);

                return _stream.WriteAsync(buffer, cancellationToken);
            }

            public override void WriteByte(byte value)
            {
                _storage.AddWriteByte();

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
        }

        private sealed class StatisticsSocket : ISocket
        {
            private readonly ISocket _socket;
            private readonly SocketStatisticsStorage _storage;

            public StatisticsSocket(ISocket socket, SocketStatisticsStorage storage)
            {
                _socket = socket;
                _storage = storage;
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

            public void Bind(EndPoint localEndPoint)
            {
                _socket.Bind(localEndPoint);
            }

            public void BindToDevice(string interfaceName)
            {
                _socket.BindToDevice(interfaceName);
            }

            public ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken)
            {
                return _socket.ConnectAsync(endPoint, cancellationToken);
            }

            public Stream GetStream()
            {
                return new StatisticsStream(
                    _socket.GetStream(),
                    _storage
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

        public StatisticsSocketsProvider(ISocketsProvider provider, SocketStatisticsStorage storage)
        {
            _provider = provider;
            _storage = storage;
        }

        public int MaxAvailableSockets => _provider.MaxAvailableSockets;
        public int CurrentAvailableSockets => _provider.CurrentAvailableSockets;

        public async Task<ISocket> Create(SocketType socketType, ProtocolType protocolType, CancellationToken cancellationToken)
        {
            return new StatisticsSocket(
                await _provider.Create(socketType, protocolType, cancellationToken),
                _storage
            );
        }
    }
}
