using BotsCommon.IO.Content;
using System;
using System.Net;

namespace BotsCommon.Net.Http
{
    public sealed class HttpResponse
    {
        public HttpResponse(
            Uri uri,
            HttpStatusCode statusCode,
            string? reasonPhrase,
            HttpHeaders headers,
            IContentReader content
        )
        {
            Uri = uri;
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            Headers = headers;
            Content = content;
        }

        public Uri Uri { get; }
        public HttpStatusCode StatusCode { get; }
        public string? ReasonPhrase { get; }
        public HttpHeaders Headers { get; }
        public IContentReader Content { get; }

        public bool IsSuccessStatusCode
        {
            get { return ((int)StatusCode >= 200) && ((int)StatusCode <= 299); }
        }

        public void EnsureSuccessStatusCode()
        {
            if (!IsSuccessStatusCode)
                throw new Exception($"Status code does not indicate success {StatusCode} ({(int)StatusCode}, {ReasonPhrase})");
        }
    }
}
