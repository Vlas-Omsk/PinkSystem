namespace PinkSystem.Net.Http.Handlers.Factories
{
    public interface IHttpRequestHandlerFactory
    {
        IHttpRequestHandler Create();
    }
}
