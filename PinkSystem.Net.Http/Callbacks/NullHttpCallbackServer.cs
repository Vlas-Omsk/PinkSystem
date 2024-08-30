using System;
using System.Diagnostics.CodeAnalysis;

namespace PinkSystem.Net.Http.Callbacks
{
    public sealed class NullHttpCallbackServer : IHttpCallbackServer
    {
        private sealed class NullHttpCallbackHandler : IHttpCallbackHandler
        {
            public bool TryCreateReceiver(string path, [NotNullWhen(true)] out IHttpCallbackReceiver? receiver)
            {
                receiver = null;
                return false;
            }
        }

        public IHttpCallbackHandler CreateHandler()
        {
            return new NullHttpCallbackHandler();
        }

        public void Dispose()
        {
        }
    }
}
