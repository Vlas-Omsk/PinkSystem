using System;

namespace BotsCommon.Net.Http.Handlers.Factories
{
    public sealed class SystemNetHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        private readonly SystemNetSocketOptions _socketOptions;
        private readonly TimeSpan _timeout;

        public SystemNetHttpRequestHandlerFactory(SystemNetSocketOptions socketOptions, TimeSpan timeout)
        {
            _socketOptions = socketOptions;
            _timeout = timeout;
        }

        public IHttpRequestHandler Create(HttpRequestHandlerOptions options)
        {
            return new SystemNetHttpRequestHandler(options, _socketOptions, _timeout);
        }
    }
}
