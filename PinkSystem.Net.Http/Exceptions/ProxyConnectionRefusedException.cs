using System;
using PinkSystem.Net.Http.Exceptions;

namespace PinkSystem.Net.Http
{
    public class ProxyConnectionRefusedException : HttpException
    {
        public ProxyConnectionRefusedException() : this("Proxy connection refused")
        {
        }

        public ProxyConnectionRefusedException(string? message) : base(message)
        {
        }

        public ProxyConnectionRefusedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
