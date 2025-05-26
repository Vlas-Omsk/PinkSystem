using PinkSystem.Net.Http.Handlers;

namespace PinkSystem.Net.Http
{
    public static class IHttpRequestHandlerExtensions
    {
        public static void CopySettingsTo(this IHttpRequestHandler self, IHttpRequestHandler target)
        {
            target.Proxy = self.Proxy;
            target.ValidateSsl = self.ValidateSsl;
            target.Timeout = self.Timeout;
        }
    }
}
