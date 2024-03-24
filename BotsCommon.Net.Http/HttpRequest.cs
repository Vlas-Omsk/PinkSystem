using BotsCommon.IO.Content;
using System;

namespace BotsCommon.Net.Http
{
    public sealed class HttpRequest
    {
        public HttpRequest(string method, Uri uri)
        {
            Method = method;
            Uri = uri;
        }

        public string Method { get; }
        public Uri Uri { get; }
        public HttpHeaders Headers { get; } = new();
        public IContentReader? Content { get; set; }
        public Version? HttpVersion { get; set; }

        public HttpRequest SetContentWithHeader(IContentReader contentReader)
        {
            Headers.Replace("Content-Type", contentReader.MimeType);

            if (contentReader.Length.HasValue)
                Headers.Replace("Content-Length", contentReader.Length.Value.ToString());

            Content = contentReader;

            return this;
        }
    }
}
