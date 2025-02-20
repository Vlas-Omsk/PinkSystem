using System;
using PinkSystem.Net.Http.Exceptions;

namespace PinkSystem.Net.Http
{
    public class HttpConnectionRefusedException : HttpException
    {
        public HttpConnectionRefusedException() : this("Http connection refused")
        {
        }

        public HttpConnectionRefusedException(string? message) : base(message)
        {
        }

        public HttpConnectionRefusedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
