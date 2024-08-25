using System.IO;
using System.Text;

namespace PinkSystem.IO.Content
{
    public class StringContentReader : IContentReader
    {
        private readonly byte[] _bytes;

        public StringContentReader(string str, string mimeType = "text/plain")
        {
            _bytes = new UTF8Encoding(false).GetBytes(str);
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
