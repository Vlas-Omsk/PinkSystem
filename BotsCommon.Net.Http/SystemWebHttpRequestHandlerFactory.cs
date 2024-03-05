using BotsCommon.Net.Http.Handlers;

namespace BotsCommon.Net.Http
{
    public sealed class SystemWebHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        public IHttpRequestHandler Create(HttpRequestHandlerOptions options)
        {
            IHttpRequestHandler handler = new SystemWebHttpRequestHandler(options);

            handler = new CompressHttpRequestHandler(handler);

            return handler;
        }
    }
}
