namespace BotsCommon.IO.Content
{
    public class ByteArrayContentReader : IContentReader
    {
        private readonly byte[] _bytes;

        public ByteArrayContentReader(byte[] bytes, string mimeType)
        {
            _bytes = bytes;
            MimeType = mimeType;
        }

        public long? Length => _bytes.LongLength;
        public string MimeType { get; }

        public Stream CreateStream()
        {
            return new MemoryStream(_bytes);
        }
    }
}
