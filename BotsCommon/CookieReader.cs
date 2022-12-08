using System;
using System.Net;
using BotsCommon.IO;

namespace BotsCommon
{
    public sealed class CookiesReader
    {
        private readonly IDataReader<string> _filePathsReader;
        private readonly List<ICookieReaderProvider> _providers = new List<ICookieReaderProvider>();

        public CookiesReader(IDataReader<string> filePaths)
        {
            _filePathsReader = filePaths;
        }

        public string Domain { get; set; }
        public bool UseExpirationTimestamp { get; set; }

        public IEnumerable<Cookie> ReadFile()
        {
            var path = _filePathsReader.Read();

            if (path == null)
                return null;

            foreach (var provider in _providers)
                if (provider.IsFileFormatSupported(path))
                    return provider.ReadAllCookies(path, Domain, UseExpirationTimestamp);

            throw new NotSupportedException("File format not supported " + path);
        }

        public void AddProvider(ICookieReaderProvider provider)
        {
            _providers.Add(provider);
        }
    }
}
