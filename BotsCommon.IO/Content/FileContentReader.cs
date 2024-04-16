using MimeTypes;
using System.IO;

namespace BotsCommon.IO.Content
{
    public sealed class FileContentReader : IContentReader
    {
        private readonly FileInfo _info;

        public long? Length => _info.Length;
        public string MimeType { get; }
        public string Path { get; }

        public FileContentReader(string path, string? mimeType = null)
        {
            Path = path;
            _info = new FileInfo(Path);
            MimeType = mimeType ?? MimeTypeMap.GetMimeType(_info.Name);
        }

        public Stream CreateStream()
        {
            return File.OpenRead(Path);
        }
    }
}
