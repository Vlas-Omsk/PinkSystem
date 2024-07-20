using BotsCommon.Net.Http.Handlers;

namespace BotsCommon.Net.Http
{
    public interface IHttpRequestHandlerWrapper
    {
        IHttpRequestHandler Wrap(IHttpRequestHandler handler);
    }
}
