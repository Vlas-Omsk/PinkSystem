using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PinkSystem.Net.Sockets;

namespace PinkSystem.Net.IO
{
    internal sealed class NetworkStream : Stream
    {
        private readonly ISocket _socket;
        private readonly IPEndPoint? _remoteEndPoint;
        private readonly bool _ownsSocket;
        private int _disposed = 0;
        private int _currentReadTimeout = -1;
        private int _currentWriteTimeout = -1;
        private int _closeTimeout = -1; // -1 = respect linger options
        private bool _readable;
        private bool _writeable;

        public NetworkStream(ISocket socket, IPEndPoint? remoteEndPoint = null, FileAccess access = FileAccess.ReadWrite, bool ownsSocket = false)
        {
            _socket = socket;
            _remoteEndPoint = remoteEndPoint;
            _ownsSocket = ownsSocket;

            if (!socket.Blocking)
                throw new NotSupportedException();

            switch (access)
            {
                case FileAccess.Read:
                    _readable = true;
                    break;
                case FileAccess.Write:
                    _writeable = true;
                    break;
                case FileAccess.ReadWrite:
                default:
                    _readable = true;
                    _writeable = true;
                    break;
            }
        }

        ~NetworkStream() => Dispose(false);

        public override bool CanRead => _readable;
        public override bool CanSeek { get; } = false;
        public override bool CanWrite => _writeable;

        public override int ReadTimeout
        {
            get
            {
                var timeout = (int)_socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout)!;

                if (timeout == 0)
                    return -1;

                return timeout;
            }
            set
            {
                if (value <= 0 && value != System.Threading.Timeout.Infinite)
                    throw new ArgumentOutOfRangeException(nameof(value));

                SetSocketTimeoutOption(SocketShutdown.Receive, value);
            }
        }

        public override int WriteTimeout
        {
            get
            {
                var timeout = (int)_socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout)!;

                if (timeout == 0)
                    return -1;

                return timeout;
            }
            set
            {
                if (value <= 0 && value != System.Threading.Timeout.Infinite)
                    throw new ArgumentOutOfRangeException(nameof(value));

                SetSocketTimeoutOption(SocketShutdown.Send, value);
            }
        }

        public bool DataAvailable
        {
            get
            {
                ThrowIfDisposed();

                return _socket.Available != 0;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override int Read(Span<byte> buffer)
        {
            throw new NotSupportedException();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ValidateBufferArguments(buffer, offset, count);
            ThrowIfDisposed();

            if (!CanRead)
                throw new InvalidOperationException();

            try
            {
                var memory = new Memory<byte>(buffer, offset, count);

                if (_remoteEndPoint == null)
                {
                    return await _socket.ReceiveAsync(
                        memory,
                        SocketFlags.None,
                        cancellationToken
                    );
                }
                else
                {
                    var result = await _socket.ReceiveFromAsync(
                        memory,
                        SocketFlags.None,
                        _remoteEndPoint,
                        cancellationToken
                    );

                    return result.ReceivedBytes;
                }
            }
            catch (Exception exception) when (!(exception is OutOfMemoryException))
            {
                throw new IOException(null, exception);
            }
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var canRead = CanRead; // Prevent race with Dispose.

            ThrowIfDisposed();

            if (!canRead)
                throw new InvalidOperationException();

            try
            {
                if (_remoteEndPoint == null)
                {
                    return await _socket.ReceiveAsync(
                        buffer,
                        SocketFlags.None,
                        cancellationToken
                    );
                }
                else
                {
                    var result = await _socket.ReceiveFromAsync(
                        buffer,
                        SocketFlags.None,
                        _remoteEndPoint,
                        cancellationToken
                    );

                    return result.ReceivedBytes;
                }
            }
            catch (Exception exception) when (!(exception is OutOfMemoryException))
            {
                throw new IOException(null, exception);
            }
        }

        public override int ReadByte()
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw new NotSupportedException();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ValidateBufferArguments(buffer, offset, count);
            ThrowIfDisposed();

            if (!CanWrite)
                throw new InvalidOperationException();

            try
            {
                if (_remoteEndPoint == null)
                {
                    return _socket.SendAsync(
                        new ReadOnlyMemory<byte>(buffer, offset, count),
                        SocketFlags.None,
                        cancellationToken
                    ).AsTask();
                }
                else
                {
                    return _socket.SendToAsync(
                        new ReadOnlyMemory<byte>(buffer, offset, count),
                        SocketFlags.None,
                        _remoteEndPoint,
                        cancellationToken
                    ).AsTask();
                }
            }
            catch (Exception exception) when (!(exception is OutOfMemoryException))
            {
                throw new IOException(null, exception);
            }
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var canWrite = CanWrite; // Prevent race with Dispose.

            ThrowIfDisposed();

            if (!canWrite)
                throw new InvalidOperationException();

            try
            {
                if (_remoteEndPoint == null)
                {
                    await _socket.SendAsync(
                        buffer,
                        SocketFlags.None,
                        cancellationToken
                    );
                }
                else
                {
                    await _socket.SendToAsync(
                        buffer,
                        SocketFlags.None,
                        _remoteEndPoint,
                        cancellationToken
                    );
                }
            }
            catch (Exception exception) when (!(exception is OutOfMemoryException))
            {
                throw new IOException(null, exception);
            }
        }

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

        public void Close(int timeout)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(timeout, -1);

            _closeTimeout = timeout;

            Dispose();
        }

        public void Close(TimeSpan timeout) => Close(ToTimeoutMilliseconds(timeout));

        private static int ToTimeoutMilliseconds(TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;

            ArgumentOutOfRangeException.ThrowIfLessThan(totalMilliseconds, -1, nameof(timeout));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(totalMilliseconds, int.MaxValue, nameof(timeout));

            return (int)totalMilliseconds;
        }

        public override void Flush()
        {
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            if (disposing)
            {
                _readable = false;
                _writeable = false;

                if (_ownsSocket)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close(TimeSpan.FromMilliseconds(_closeTimeout));
                }
            }

            base.Dispose(disposing);
        }

        private void SetSocketTimeoutOption(SocketShutdown mode, int timeout)
        {
            if (timeout < 0)
                timeout = 0; // -1 becomes 0 for the winsock stack

            if (mode == SocketShutdown.Send || mode == SocketShutdown.Both)
            {
                if (timeout != _currentWriteTimeout)
                {
                    _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout);
                    _currentWriteTimeout = timeout;
                }
            }

            if (mode == SocketShutdown.Receive || mode == SocketShutdown.Both)
            {
                if (timeout != _currentReadTimeout)
                {
                    _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
                    _currentReadTimeout = timeout;
                }
            }
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed == 1, this);
        }
    }
}
