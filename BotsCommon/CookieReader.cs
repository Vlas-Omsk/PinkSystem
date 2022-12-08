using System;
using System.Net;
using BotsCommon.IO;

namespace BotsCommon
{
    public sealed class CookieReader
    {
        private readonly IDataReader<string> _filePathsReader;
        private readonly List<ICookieReaderProvider> _providers = new List<ICookieReaderProvider>();

        public CookieReader(IDataReader<string> filePaths)
        {
            _filePathsReader = filePaths;
        }

        public IEnumerable<Cookie> ReadFile(string domain = null, bool useExpirationTimestamp = false)
        {
            var path = _filePathsReader.Read();

            if (path == null)
                return null;

            foreach (var provider in _providers)
                if (provider.IsFileFormatSupported(path))
                    return provider.ReadAllCookies(path, domain, useExpirationTimestamp);

            throw new NotSupportedException("File format not supported " + path);
        }

        public void AddProvider(ICookieReaderProvider provider)
        {
            _providers.Add(provider);
        }
    }
}
