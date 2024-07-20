namespace BotsCommon.Net.Http.Handlers.Factories
{
    public sealed class HttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        private readonly IHttpRequestHandlerFactory _httpRequestHandlerFactory;
        private readonly IHttpRequestHandlerWrapper _httpRequestHandlerWrapper;

        public HttpRequestHandlerFactory(
            IHttpRequestHandlerFactory httpRequestHandlerFactory,
            IHttpRequestHandlerWrapper httpRequestHandlerWrapper
        )
        {
            _httpRequestHandlerFactory = httpRequestHandlerFactory;
            _httpRequestHandlerWrapper = httpRequestHandlerWrapper;
        }

        public IHttpRequestHandler Create(HttpRequestHandlerOptions options)
        {
            var handler = _httpRequestHandlerFactory.Create(options);

            return _httpRequestHandlerWrapper.Wrap(handler);
        }
    }
}
