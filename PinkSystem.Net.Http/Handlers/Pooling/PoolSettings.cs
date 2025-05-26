using System;

namespace PinkSystem.Net.Http.Handlers.Pooling
{
    public sealed record PoolSettings
    {
        public required Proxy? Proxy { get; init; }
        public required bool ValidateSsl { get; init; }
        public required TimeSpan Timeout { get; init; }

        public void ApplyTo(IHttpRequestHandler handler)
        {
            handler.Proxy = Proxy;
            handler.ValidateSsl = ValidateSsl;
            handler.Timeout = Timeout;
        }
    }
}
