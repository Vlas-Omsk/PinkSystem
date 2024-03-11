using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http.Callbacks
{
    public sealed class HttpCallbackReceiver
    {
        private readonly Uri _uri;
        private readonly ILogger<HttpCallbackReceiver> _logger;
        private readonly HttpListener _listener;
        private readonly List<IHttpCallbackHandler> _handlers = new();
        private readonly object _lock = new();
        private Task? _task;

        public HttpCallbackReceiver(string externalHost, int port, ILogger<HttpCallbackReceiver> logger)
        {
            _uri = new Uri($"http://{externalHost}:{port}/");
            _logger = logger;

            _listener = new();
            _listener.Prefixes.Add($"http://+:{port}/");
        }

        public bool IsListening { get; private set; }

        public Uri GetUri(string path)
        {
            return new Uri(_uri, path);
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            IsListening = true;

            _listener.Start();

            _task = Task.Run(async () =>
            {
                while (true)
                {
                    var context = await _listener.GetContextAsync();

                    _ = Task.Run(() =>
                    {
                        try
                        {
                            IHttpCallbackHandler[] handlers;

                            lock (_lock)
                                handlers = _handlers.ToArray();

                            foreach (var handler in handlers)
                            {
                                if (handler.TryHandle(context.Request))
                                {
                                    lock (_lock)
                                        _handlers.Remove(handler);

                                    break;
                                }
                            }

                            context.Response.Close();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error when handling callback request");
                        }
                    });
                }
            });

            var testUrl = GetUri("/test");

            var handler = new HttpCallbackHandler<bool>(x =>
            {
                if (!x.HttpMethod.Equals("GET") ||
                    x.Url != testUrl)
                    return new HttpCallbackHandlerResponse<bool>(false, false);

                return new HttpCallbackHandlerResponse<bool>(true, true);
            });

            AddHandler(handler);

            using var httpClient = new HttpClient();

            await httpClient.GetAsync(testUrl);

            try
            {
                handler.Wait(TimeSpan.FromSeconds(10), cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception("Callback server self test failed.", ex);
            }
        }

        public void AddHandler<T>(HttpCallbackHandler<T> handler)
        {
            lock (_lock)
                _handlers.Add(handler);
        }
    }
}
