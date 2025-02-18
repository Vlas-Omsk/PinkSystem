using System;

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
}
