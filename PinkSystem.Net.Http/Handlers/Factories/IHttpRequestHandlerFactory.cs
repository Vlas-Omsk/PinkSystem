using PinkSystem.Net.Http.Handlers;

namespace PinkSystem.Net.Http.Handlers.Factories
{
    public interface IHttpRequestHandlerFactory
    {
        IHttpRequestHandler Create(HttpRequestHandlerOptions options);
    }
}
