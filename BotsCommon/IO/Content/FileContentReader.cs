using MimeTypes;

namespace BotsCommon.IO.Content
{
    public sealed class FileContentReader : IContentReader
    {
        private readonly string _path;
        private readonly FileInfo _info;

        public long Length => _info.Length;
        public string MimeType { get; }

        public FileContentReader(string path)
        {
            _path = path;
            _info = new FileInfo(_path);
            MimeType = MimeTypeMap.GetMimeType(_info.Name);
        }

        public Stream CreateStream()
        {
            return File.OpenRead(_path);
        }
    }
}
