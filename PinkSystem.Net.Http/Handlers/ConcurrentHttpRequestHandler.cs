using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers
{
    public sealed class ConcurrentHttpRequestHandler : IHttpRequestHandler
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly int _concurrency;
        private readonly IHttpRequestHandler _handler;

        public ConcurrentHttpRequestHandler(IHttpRequestHandler handler, int concurrency)
        {
            _semaphore = new(concurrency, concurrency);
            _concurrency = concurrency;
            _handler = handler;
        }

        public HttpRequestHandlerOptions Options => _handler.Options;

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                return await _handler.SendAsync(request, cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public IHttpRequestHandler Clone()
        {
            return new ConcurrentHttpRequestHandler(_handler.Clone(), _concurrency);
        }

        public void Dispose()
        {
            _handler.Dispose();
            _semaphore.Dispose();
        }
    }
}
