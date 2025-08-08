using System;
using System.Net;

namespace PinkSystem.Net.Http.Exceptions
{
    public class HttpException : Exception
    {
        public HttpException() : this("Http exception occurred")
        {
        }

        public HttpException(string? message) : base(message)
        {
        }

        public HttpException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    public class HttpErrorStatusCodeException : HttpException
    {
        public HttpErrorStatusCodeException(HttpStatusCode statusCode, string? reasonPhrase) : this(statusCode, reasonPhrase, null)
        {
        }

        public HttpErrorStatusCodeException(HttpStatusCode statusCode, string? reasonPhrase, string? message) : base(message == null ? $"Status code does not indicate success {statusCode} ({(int)statusCode}, {reasonPhrase})" : message)
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
        }

        public HttpErrorStatusCodeException(HttpStatusCode statusCode, string? reasonPhrase, string? message, Exception? innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
        }

        public HttpStatusCode StatusCode { get; }
        public string? ReasonPhrase { get; }
    }
}
