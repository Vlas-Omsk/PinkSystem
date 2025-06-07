using System;

namespace PinkSystem.Net.Http.Handlers
{
    public interface IHttpRequestHandlerOptions : IEquatable<IHttpRequestHandlerOptions>
    {
        Proxy? Proxy { get; }
        TimeSpan Timeout { get; }
    }
}
