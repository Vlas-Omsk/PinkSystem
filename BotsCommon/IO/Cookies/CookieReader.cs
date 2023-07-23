using System.Net;

namespace BotsCommon.IO.Cookies
{
    public sealed class CookieReader
    {
        private readonly List<ICookieReader> _providers = new();

        public void AddProvider(ICookieReader provider)
        {
            _providers.Add(provider);
        }

        public IEnumerable<Cookie> ReadAll(string path)
        {
            foreach (var provider in _providers)
                if (provider.IsFileFormatSupported(path))
                    return provider.ReadAllCookies(path);

            throw new NotSupportedException("File format not supported " + path);
        }
    }
}
