using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers
{
    public sealed class ConcurrentHttpRequestHandler : ExtensionHttpRequestHandler
    {
        private readonly SemaphoreSlim _semaphore;

        public ConcurrentHttpRequestHandler(IHttpRequestHandler handler, int concurrency) : base(handler)
        {
            _semaphore = new(concurrency, concurrency);
        }

        public override async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                return await Handler.SendAsync(request, cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            _semaphore.Dispose();
        }
    }
}
