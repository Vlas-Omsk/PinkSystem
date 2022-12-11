using System;
using System.Net;

namespace BotsCommon.IO
{
    public sealed class CookiesDataReader : IDataReader<IEnumerable<Cookie>>
    {
        private readonly IDataReader<string> _filePathsReader;
        private readonly List<ICookieReaderProvider> _providers = new List<ICookieReaderProvider>();

        public CookiesDataReader(IDataReader<string> filePaths)
        {
            _filePathsReader = filePaths;
        }

        public string Domain { get; set; }
        public bool UseExpirationTimestamp { get; set; }

        public int Index => _filePathsReader.Index;
        public int Length => _filePathsReader.Length;

        public void AddProvider(ICookieReaderProvider provider)
        {
            _providers.Add(provider);
        }

        public IEnumerable<Cookie> Read()
        {
            var path = _filePathsReader.Read();

            if (path == null)
                return null;

            try
            {
                foreach (var provider in _providers)
                    if (provider.IsFileFormatSupported(path))
                        return provider.ReadAllCookies(path, Domain, UseExpirationTimestamp).ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while processing cookie file " + path, ex);
            }

            throw new NotSupportedException("File format not supported " + path);
        }

        public void Reset()
        {
            _filePathsReader.Reset();
        }
    }
}
