namespace PinkSystem.Net.Http.Handlers
{
    public interface IHttpRequestHandlerFactory
    {
        IHttpRequestHandler Create(IHttpRequestHandlerOptions? options = null);
    }
}
