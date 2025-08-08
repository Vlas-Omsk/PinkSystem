using System;
using System.IO;
using CommunityToolkit.HighPerformance;

namespace PinkSystem.IO.Content
{
    public class ByteArrayContentReader : IContentReader
    {
        private readonly ReadOnlyMemory<byte> _bytes;

        public ByteArrayContentReader(ReadOnlyMemory<byte> bytes, string mimeType)
        {
            _bytes = bytes;
            MimeType = mimeType;
        }

        public long? Length => _bytes.Length;
        public string MimeType { get; }

        public Stream CreateStream()
        {
            return _bytes.AsStream();
        }
    }
}
