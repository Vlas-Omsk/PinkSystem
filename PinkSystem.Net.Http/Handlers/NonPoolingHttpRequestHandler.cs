using PinkSystem.Net.Http.Handlers.Factories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers
{
    public sealed class NonPoolingHttpRequestHandler : IHttpRequestHandler
    {
        private readonly IHttpRequestHandlerFactory _handlerFactory;

        public NonPoolingHttpRequestHandler(IHttpRequestHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory;

            using var handler = _handlerFactory.Create();

            Proxy = handler.Proxy;
            ValidateSsl = handler.ValidateSsl;
            Timeout = handler.Timeout;
        }

        public Proxy? Proxy { get; set; }
        public bool ValidateSsl { get; set; }
        public TimeSpan Timeout { get; set; }

        public Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            var handler = _handlerFactory.Create();

            handler.Proxy = Proxy;
            handler.ValidateSsl = ValidateSsl;
            handler.Timeout = Timeout;

            return handler.SendAsync(request, cancellationToken);
        }

        public IHttpRequestHandler Clone()
        {
            var handler = new NonPoolingHttpRequestHandler(_handlerFactory);

            this.CopySettingsTo(handler);

            return handler;
        }

        public void Dispose()
        {
        }
    }
}
