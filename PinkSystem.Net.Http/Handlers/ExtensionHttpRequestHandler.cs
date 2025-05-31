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

        public abstract Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken);

        public virtual void Dispose()
        {
            Handler.Dispose();
        }
    }
}
