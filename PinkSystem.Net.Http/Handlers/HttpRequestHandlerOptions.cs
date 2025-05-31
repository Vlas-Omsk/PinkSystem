using System;

namespace PinkSystem.Net.Http.Handlers
{
    public sealed record HttpRequestHandlerOptions : IHttpRequestHandlerOptions
    {
        public Proxy? Proxy { get; init; }
        public TimeSpan Timeout { get; init; } = HttpTimeout.Default;

        public bool Equals(IHttpRequestHandlerOptions? other)
        {
            return ((object)this).Equals(other);
        }
    }
}
