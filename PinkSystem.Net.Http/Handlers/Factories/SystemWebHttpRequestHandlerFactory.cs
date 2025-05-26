namespace PinkSystem.Net.Http.Handlers.Factories
{
    public sealed class SystemWebHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        public IHttpRequestHandler Create()
        {
            return new SystemWebHttpRequestHandler();
        }
    }
}
