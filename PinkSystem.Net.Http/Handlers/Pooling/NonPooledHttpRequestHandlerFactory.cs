using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers.Pooling
{
    public sealed class NonPooledHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        private readonly IHttpRequestHandlerFactory _handlerFactory;

        private sealed class NonPooledHttpRequestHandler : IHttpRequestHandler
        {
            private readonly IHttpRequestHandlerOptions? _options;
            private readonly IHttpRequestHandlerFactory _handlerFactory;

            public NonPooledHttpRequestHandler(IHttpRequestHandlerOptions? options, IHttpRequestHandlerFactory handlerFactory)
            {
                _options = options;
                _handlerFactory = handlerFactory;
            }

            public Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
            {
                using var handler = _handlerFactory.Create(_options);

                return handler.SendAsync(request, cancellationToken);
            }

            public void Dispose()
            {
            }
        }

        public NonPooledHttpRequestHandlerFactory(IHttpRequestHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory;
        }

        public IHttpRequestHandler Create(IHttpRequestHandlerOptions? options)
        {
            return new NonPooledHttpRequestHandler(
                options,
                _handlerFactory
            );
        }
    }
}
