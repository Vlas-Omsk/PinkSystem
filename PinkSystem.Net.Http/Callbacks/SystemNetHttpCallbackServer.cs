using Microsoft.Extensions.Logging;
using PinkSystem.IO.Content;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Callbacks
{
    public sealed class SystemNetHttpCallbackServer : IHttpCallbackServer
    {
        private readonly Uri _uri;
        private readonly ILogger<SystemNetHttpCallbackServer> _logger;
        private readonly HttpListener _listener;
        private readonly ConcurrentDictionary<string, SystemNetHttpCallbackHandler> _handlers = new();
        private Task? _task;

        private sealed class SystemNetHttpCallbackHandler : IHttpCallbackHandler
        {
            private readonly Uri _uri;
            private readonly ConcurrentDictionary<Uri, SystemNetHttpCallbackReceiver> _receivers = new();

            public SystemNetHttpCallbackHandler(Uri uri)
            {
                _uri = uri;
            }

            public IHttpCallbackReceiver CreateReceiver(string path)
            {
                var uri = new Uri(_uri, path);

                if (!uri.AbsolutePath.StartsWith(_uri.AbsolutePath))
                    throw new ArgumentException("Path cannot have root path");

                var receiver = new SystemNetHttpCallbackReceiver(this, uri);

                _receivers.TryAdd(uri, receiver);

                return receiver;
            }

            internal void RemoveReceiver(SystemNetHttpCallbackReceiver receiver)
            {
                _receivers.TryRemove(receiver.ExternalUri, out _);
            }

            internal void ProcessRequest(HttpRequest request)
            {
                if (!_receivers.TryGetValue(request.Uri, out var receiver))
                    return;

                receiver.EnqueueRequest(request);
            }
        }

        private sealed class SystemNetHttpCallbackReceiver : IHttpCallbackReceiver
        {
            private readonly SystemNetHttpCallbackHandler _handler;
            private readonly ConcurrentQueue<HttpRequest> _requests = new();
            private readonly SemaphoreSlim _waiter = new(0);

            public SystemNetHttpCallbackReceiver(SystemNetHttpCallbackHandler handler, Uri externalUri)
            {
                _handler = handler;
                ExternalUri = externalUri;
            }

            public Uri ExternalUri { get; }

            public async Task<HttpRequest> Receive(TimeSpan timeout, CancellationToken cancellationToken)
            {
                await _waiter.WaitAsync(timeout, cancellationToken);

                if (!_requests.TryDequeue(out var request))
                    throw new Exception("Requests queue was empty");

                return request;
            }

            internal void EnqueueRequest(HttpRequest request)
            {
                _requests.Enqueue(request);
                _waiter.Release();
            }

            public void Dispose()
            {
                _handler.RemoveReceiver(this);
            }
        }

        public SystemNetHttpCallbackServer(string externalHost, int port, ILogger<SystemNetHttpCallbackServer> logger)
        {
            _uri = new Uri($"http://{externalHost}:{port}/");
            _logger = logger;

            _listener = new();
            _listener.Prefixes.Add($"http://+:{port}/");
        }

        public bool IsListening { get; private set; }

        public async Task Start(CancellationToken cancellationToken)
        {
            if (IsListening)
                throw new InvalidOperationException("server listening");

            _listener.Start();

            _task = Task.Run(async () =>
            {
                while (true)
                {
                    var context = await _listener.GetContextAsync().ConfigureAwait(false);

                    try
                    {
                        HandleRequest(context);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error when handling callback request");

                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Response.Close();
                    }
                }
            });

            IsListening = true;

            await Selftest(cancellationToken);
        }

        public IHttpCallbackHandler CreateHandler()
        {
            var path = '/' + Guid.NewGuid().ToString();
            var uri = new Uri(_uri, path);
            var handler =  new SystemNetHttpCallbackHandler(uri);

            _handlers.TryAdd(path, handler);

            return handler;
        }

        private async Task Selftest(CancellationToken cancellationToken)
        {
            var handler = CreateHandler();

            using var receiver = handler.CreateReceiver("test");

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(receiver.ExternalUri, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            HttpRequest request;

            try
            {
                request = await receiver.Receive(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Http callback receiver selft test failed", ex);
            }

            if (!request.Method.Equals("GET") ||
                request.Uri != receiver.ExternalUri)
                throw new Exception("Http callback receiver selft test failed");
        }

        private void HandleRequest(HttpListenerContext context)
        {
            var request = new HttpRequest(context.Request.HttpMethod, context.Request.Url!);

            if (context.Request.HasEntityBody)
            {
                var buffer = ArrayPool<byte>.Shared.Rent(16 * 1024);

                try
                {
                    using var memoryStream = new MemoryStream();

                    int read;

                    while ((read = context.Request.InputStream.Read(buffer, 0, buffer.Length)) > 0)
                        memoryStream.Write(buffer, 0, read);

                    request.Content = new ByteArrayContentReader(
                        memoryStream.ToReadOnlyMemory(),
                        context.Request.ContentType ?? "application/octet-stream"
                    );
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            foreach (string key in context.Request.Headers.Keys)
                request.Headers.Add(key, context.Request.Headers[key] ?? "");

            foreach (var (path, handler) in _handlers)
            {
                if (!request.Uri.AbsolutePath.StartsWith(path))
                    continue;

                handler.ProcessRequest(request);
                break;
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.Close();
        }

        public void Dispose()
        {
            _listener.Stop();

            IsListening = false;
        }
    }
}
