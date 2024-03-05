using BotsCommon.Net.Http.Handlers;

namespace BotsCommon.Net.Http
{
    public interface IHttpRequestHandlerFactory
    {
        IHttpRequestHandler Create(HttpRequestHandlerOptions options);
    }
}
