using PinkSystem.Net.Http.Handlers;

namespace PinkSystem.Net.Http
{
    public interface IHttpRequestHandlerWrapper
    {
        IHttpRequestHandler Wrap(IHttpRequestHandler handler);
    }
}
