using System;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers
{
    public abstract class ExtensionHttpRequestHandler : IHttpRequestHandler
    {
        protected ExtensionHttpRequestHandler(IHttpRequestHandler handler)
        {
            Handler = handler;
        }

        protected IHttpRequestHandler Handler { get; }

        public virtual Proxy? Proxy
        {
            get => Handler.Proxy;
            set => Handler.Proxy = value;
        }

        public virtual bool ValidateSsl
        {
            get => Handler.ValidateSsl;
            set => Handler.ValidateSsl = value;
        }

        public virtual TimeSpan Timeout
        {
            get => Handler.Timeout;
            set => Handler.Timeout = value;
        }

        public abstract Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken);

        public abstract IHttpRequestHandler Clone();

        public virtual void Dispose()
        {
            Handler.Dispose();
        }
    }
}
