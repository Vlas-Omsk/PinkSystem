using System;

namespace BotsCommon.Net.Http.Handlers.Factories
{
    public sealed class SystemNetNonPooledHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        private readonly SystemNetSocketOptions _socketOptions;
        private readonly TimeSpan _timeout;

        public SystemNetNonPooledHttpRequestHandlerFactory(SystemNetSocketOptions socketOptions, TimeSpan timeout)
        {
            _socketOptions = socketOptions;
            _timeout = timeout;
        }

        public IHttpRequestHandler Create(HttpRequestHandlerOptions options)
        {
            return new SystemNetNonPooledHttpRequestHandler(options, _socketOptions, _timeout);
        }
    }
}
