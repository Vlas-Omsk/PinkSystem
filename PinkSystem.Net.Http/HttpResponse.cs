using PinkSystem.IO.Content;
using PinkSystem.Net.Http.Exceptions;
using System;
using System.Net;

namespace PinkSystem.Net.Http
{
    public sealed class HttpResponse
    {
        public required Uri Uri { get; init; }
        public required HttpStatusCode StatusCode { get; init; }
        public string? ReasonPhrase { get; init; }
        public IReadOnlyHttpHeaders Headers { get; init; } = HttpHeaders.Empty;
        public required IContentReader Content { get; init; }

        public bool IsSuccessStatusCode => ((int)StatusCode >= 200) && ((int)StatusCode <= 299);

        public void EnsureSuccessStatusCode()
        {
            if (!IsSuccessStatusCode)
                throw new HttpErrorStatusCodeException(StatusCode, ReasonPhrase);
        }
    }
}
