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

        private sealed class StatisticsSocket : ExtensionSocket
        {
            private readonly SocketStatisticsStorage _storage;

            public StatisticsSocket(ISocket socket, SocketStatisticsStorage storage) : base(socket)
            {
                _storage = storage;
            }

            public override async ValueTask<int> ReceiveAsync(Memory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken)
            {
                var amount = await base.ReceiveAsync(buffer, socketFlags, cancellationToken);

                _storage.AddReadBytes(amount);

                return amount;
            }

            public override async ValueTask<SocketReceiveFromResult> ReceiveFromAsync(Memory<byte> buffer, SocketFlags socketFlags, EndPoint remoteEndPoint, CancellationToken cancellationToken)
            {
                var result = await base.ReceiveFromAsync(buffer, socketFlags, remoteEndPoint, cancellationToken);

                _storage.AddReadBytes(result.ReceivedBytes);

                return result;
            }

            public override ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken)
            {
                _storage.AddWriteBytes(buffer.Length);

                return base.SendAsync(buffer, socketFlags, cancellationToken);
            }

            public override ValueTask<int> SendToAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, EndPoint remoteEndPoint, CancellationToken cancellationToken)
            {
                _storage.AddWriteBytes(buffer.Length);

                return base.SendToAsync(buffer, socketFlags, remoteEndPoint, cancellationToken);
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
