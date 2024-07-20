using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http.Handlers
{
    public sealed class ConcurrentHttpRequestHandler : IHttpRequestHandler
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly IHttpRequestHandler _handler;

        public ConcurrentHttpRequestHandler(IHttpRequestHandler handler)
        {
            _semaphore = new(ServicePointManager.DefaultConnectionLimit, ServicePointManager.DefaultConnectionLimit);
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
            return new ConcurrentHttpRequestHandler(_handler.Clone());
        }

        public void Dispose()
        {
            _handler.Dispose();
            _semaphore.Dispose();
        }
    }
}
