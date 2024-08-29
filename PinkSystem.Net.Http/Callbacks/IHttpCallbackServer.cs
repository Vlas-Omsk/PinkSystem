using System;

namespace PinkSystem.Net.Http.Callbacks
{
    public interface IHttpCallbackServer : IDisposable
    {
        IHttpCallbackHandler CreateHandler();
    }
}
