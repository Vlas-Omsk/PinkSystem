using System;
using System.Net;

namespace BotsCommon.Net
{
    public sealed class CookiesReader
    {
        private readonly List<ICookieReaderProvider> _providers = new List<ICookieReaderProvider>();

        public string Domain { get; set; }
        public bool UseExpirationTimestamp { get; set; }

        public void AddProvider(ICookieReaderProvider provider)
        {
            _providers.Add(provider);
        }

        public IEnumerable<Cookie> ReadAll(string path)
        {
            foreach (var provider in _providers)
                if (provider.IsFileFormatSupported(path))
                    return provider.ReadAllCookies(path, Domain, UseExpirationTimestamp);

            throw new NotSupportedException("File format not supported " + path);
        }
    }
}
