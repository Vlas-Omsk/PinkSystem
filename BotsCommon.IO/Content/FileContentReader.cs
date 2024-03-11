using MimeTypes;
using System.IO;

namespace BotsCommon.IO.Content
{
    public sealed class FileContentReader : IContentReader
    {
        private readonly string _path;
        private readonly FileInfo _info;

        public long? Length => _info.Length;
        public string MimeType { get; }

        public FileContentReader(string path, string? mimeType = null)
        {
            _path = path;
            _info = new FileInfo(_path);
            MimeType = mimeType ?? MimeTypeMap.GetMimeType(_info.Name);
        }

        public Stream CreateStream()
        {
            return File.OpenRead(_path);
        }
    }
}
