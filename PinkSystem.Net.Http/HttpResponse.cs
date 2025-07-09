using PinkSystem.IO.Content;
using PinkSystem.Net.Http.Exceptions;
using System;
using System.Net;

namespace PinkSystem.Net.Http
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

        public bool IsSuccessStatusCode => ((int)StatusCode >= 200) && ((int)StatusCode <= 299);

        public void EnsureSuccessStatusCode()
        {
            if (!IsSuccessStatusCode)
                throw new HttpErrorStatusCodeException(StatusCode, ReasonPhrase);
        }
    }
}
