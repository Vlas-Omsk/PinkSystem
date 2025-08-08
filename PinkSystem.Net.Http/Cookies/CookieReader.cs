using System;
using System.Collections.Generic;
using System.Net;
using PinkSystem.IO.Content;

namespace PinkSystem.Net.Http.Cookies
{
    public sealed class CookieReader
    {
        public List<ICookieProvider> Providers { get; } = new();

        public IEnumerable<Cookie> ReadAll(IContentReader reader)
        {
            foreach (var provider in Providers)
                if (provider.IsSupported(reader))
                    return provider.ReadAll(reader);

            throw new NotSupportedException("Cookie format not supported " + reader.MimeType);
        }
    }
}
