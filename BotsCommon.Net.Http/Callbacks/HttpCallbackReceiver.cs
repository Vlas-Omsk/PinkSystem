using BotsCommon.IO.Content;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http.Callbacks
{
    public sealed class HttpCallbackReceiver : IHttpCallbackReceiver
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
                    var context = await _listener.GetContextAsync().ConfigureAwait(false);

                    _ = Task.Run(() =>
                    {
                        try
                        {
                            IHttpCallbackHandler[] handlers;

                            lock (_lock)
                                handlers = _handlers.ToArray();

                            foreach (var handler in handlers)
                            {
                                var request = new HttpRequest(context.Request.HttpMethod, context.Request.Url!);

                                if (context.Request.HasEntityBody)
                                {
                                    byte[] buffer = new byte[16 * 1024];

                                    using var memoryStream = new MemoryStream();
                                    
                                    int read;

                                    while ((read = context.Request.InputStream.Read(buffer, 0, buffer.Length)) > 0)
                                        memoryStream.Write(buffer, 0, read);

                                    request.Content = new ByteArrayContentReader(
                                        memoryStream.ToReadOnlyMemory(),
                                        context.Request.ContentType ?? "application/octet-stream"
                                    );
                                }

                                foreach (string key in context.Request.Headers.Keys)
                                    request.Headers.Add(key, context.Request.Headers[key] ?? "");

                                if (handler.TryHandle(request))
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
                if (!x.Method.Equals("GET") ||
                    x.Uri != testUrl)
                    return new HttpCallbackHandlerResponse<bool>(false, false);

                return new HttpCallbackHandlerResponse<bool>(true, true);
            });

            AddHandler(handler);

            using var httpClient = new HttpClient();

            await httpClient.GetAsync(testUrl).ConfigureAwait(false);

            try
            {
                await handler.Wait(TimeSpan.FromSeconds(10), cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception("Callback server self test failed.", ex);
            }
        }

        public void AddHandler(IHttpCallbackHandler handler)
        {
            lock (_lock)
                _handlers.Add(handler);
        }
    }
}
