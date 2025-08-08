using System;
using PinkSystem.IO.Content;

namespace PinkSystem.Net.Http
{
    public sealed class HttpRequest
    {
        public required string Method { get; init; }
        public required Uri Uri { get; init; }
        public IReadOnlyHttpHeaders Headers { get; init; } = HttpHeaders.Empty;
        public IContentReader? Content { get; init; }
        public Version? Version { get; init; }
    }

    public sealed class HttpRequestBuilder
    {
        public required string Method { get; init; }
        public required Uri Uri { get; init; }
        public HttpHeaders Headers { get; set; } = new();
        public IContentReader? Content { get; set; }
        public Version? Version { get; set; }

        public HttpRequestBuilder SetContentWithHeader(IContentReader contentReader)
        {
            Headers.Replace("Content-Type", contentReader.MimeType);

            if (contentReader.Length.HasValue)
                Headers.Replace("Content-Length", contentReader.Length.Value.ToString());

            Content = contentReader;

            return this;
        }

        public HttpRequest Build()
        {
            return new HttpRequest()
            {
                Method = Method,
                Uri = Uri,
                Headers = Headers,
                Content = Content,
                Version = Version
            };
        }
    }
}
