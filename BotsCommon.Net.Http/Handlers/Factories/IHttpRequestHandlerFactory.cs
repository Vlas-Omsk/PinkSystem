using BotsCommon.Net.Http.Handlers;

namespace BotsCommon.Net.Http.Handlers.Factories
{
    public interface IHttpRequestHandlerFactory
    {
        IHttpRequestHandler Create(HttpRequestHandlerOptions options);
    }
}
