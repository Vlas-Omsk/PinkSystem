using System;
using System.Buffers;
using System.IO;

namespace PinkSystem.Text
{
    public sealed class BufferedTextReader : TextReader
    {
        private const int _defaultBufferSize = 4096;
        private readonly TextReader _reader;
        private char[] _buffer = new char[_defaultBufferSize];
        private int _dataPosition = 0;
        private int _dataSize = 0;

        public BufferedTextReader(TextReader inner)
        {
            _reader = inner;
        }

        private Span<char> DataSpan => _buffer.AsSpan(_dataPosition, _dataSize);

        public override int Peek()
        {
            var span = PeekSpanUnsafe(1);

            if (span.Length == 0)
                return -1;

            return span[0];
        }

        public string Peek(int amount)
        {
            return new string(PeekSpanUnsafe(amount));
        }

        public char[] PeekChars(int amount)
        {
            return PeekSpanUnsafe(amount).ToArray();
        }

        internal ReadOnlySpan<char> PeekSpanUnsafe(int amount)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(amount, nameof(amount));

            if (amount == 0)
                return string.Empty;

            while (_dataSize < amount)
            {
                int freeSize = _buffer.Length - _dataSize;
                int freeSizeAfter = freeSize - _dataPosition;

                if (freeSizeAfter < amount)
                {
                    if (freeSize >= amount)
                        MoveDataToBufferStart();
                    else
                        GrowBuffer(amount);
                }
                else
                {
                    var emptySpan = _buffer.AsSpan(_dataPosition + _dataSize);
                    var readedSize = _reader.Read(emptySpan);

                    if (readedSize == 0)
                        break;

                    _dataSize += readedSize;
                }
            }

            return DataSpan.Slice(0, Math.Min(_dataSize, amount));
        }

        private void GrowBuffer(int minSize)
        {
            var newBuffer = new char[(int)Math.Ceiling((decimal)minSize / _defaultBufferSize)];

            DataSpan.CopyTo(newBuffer);

            _buffer = newBuffer;
            _dataPosition = 0;
        }

        private void MoveDataToBufferStart()
        {
            var tempBuffer = ArrayPool<char>.Shared.Rent(_dataSize);

            try
            {
                DataSpan.CopyTo(tempBuffer);

                tempBuffer.AsSpan(0, _dataSize).CopyTo(_buffer);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(tempBuffer);
            }

            _dataPosition = 0;
        }

        public override int Read()
        {
            var tempBuffer = ArrayPool<char>.Shared.Rent(1);

            try
            {
                var readedSize = Read(tempBuffer, 0, 1);

                if (readedSize == 0)
                    return -1;

                return tempBuffer[0];
            }
            finally
            {
                ArrayPool<char>.Shared.Return(tempBuffer);
            }
        }

        public override int Read(char[] buffer, int index, int count)
        {
            if (count <= _dataSize)
            {
                DataSpan.Slice(0, count).CopyTo(buffer.AsSpan(index, count));

                _dataPosition += count;
                _dataSize -= count;

                if (_dataSize == 0)
                    _dataPosition = 0;

                return count;
            }

            var readedSize = 0;

            if (_dataSize > 0)
            {
                DataSpan.CopyTo(buffer.AsSpan(index, _dataSize));

                index += _dataSize;
                count -= _dataSize;

                readedSize += _dataSize;

                _dataPosition = 0;
                _dataSize = 0;
            }

            return readedSize + _reader.Read(buffer, index, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _reader.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
