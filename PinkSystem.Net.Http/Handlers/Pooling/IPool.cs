using System;

namespace PinkSystem.Net.Http.Handlers.Pooling
{
    public interface IPool : IDisposable
    {
        IPoolMap GetMap(IHttpRequestHandlerOptions? options);
    }
}
