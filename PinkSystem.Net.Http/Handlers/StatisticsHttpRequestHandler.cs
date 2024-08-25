using PinkSystem.Net.Http.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers
{
    public sealed class StatisticsHttpRequestHandler : IHttpRequestHandler
    {
        private readonly IHttpRequestHandler _handler;
        private readonly Storage _storage;

        private enum FailType
        {
            Timeout,
            Proxy,
            Other
        }

        public sealed class Storage
        {
            private readonly StatisticsSocketsProvider _socketsProvider;
            private long _sent = 0;
            private long _success = 0;
            private readonly ConcurrentDictionary<FailType, long> _failed = new();
            private long _timeElapsed = 0;
            private long _sentBytes = 0;
            private long _receivedBytes = 0;
            private ILogger<Storage> _logger;
            private Task? _task;

            public Storage(StatisticsSocketsProvider socketsProvider, ILogger<Storage> logger)
            {
                _socketsProvider = socketsProvider;
                _logger = logger;

                _task = Task.Run(
                    async () =>
                    {
                        while (true)
                        {
                            try
                            {
                                var ping = 0L;
                                var sentSpeed = "?";
                                var receiveSpeed = "?";
                                var sent = _sent;
                                var timeElapsed = _timeElapsed;

                                if (sent > 0)
                                    ping = timeElapsed / sent;

                                if (timeElapsed > 0)
                                {
                                    sentSpeed = (_sentBytes / (timeElapsed / 1000)).FormatBytes();
                                    receiveSpeed = (_receivedBytes / (timeElapsed / 1000)).FormatBytes();
                                }

                                _logger.LogInformation(
                                    "Http statistics" + Environment.NewLine +
                                    "    Success: {success}" + Environment.NewLine +
                                    "    Failed: {failed}" + Environment.NewLine +
                                    "    Ping: {ping} ms" + Environment.NewLine +
                                    "    Sent: {sent} ({sentSpeed}/s), Socket: {sentSocket}" + Environment.NewLine +
                                    "    Receive: {receive} ({receiveSpeed}/s), Socket: {receiveSocket}",
                                    _success,
                                    string.Join(", ", _failed.Select(x => $"{x.Key}: {x.Value}")),
                                    ping,
                                    _sentBytes.FormatBytes(),
                                    sentSpeed,
                                     _socketsProvider.WriteBytes.FormatBytes(),
                                    _receivedBytes.FormatBytes(),
                                    receiveSpeed,
                                     _socketsProvider.ReadBytes.FormatBytes()
                                );
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error when counting http statistics");
                            }

                            await Task.Delay(5_000);
                        }
                    }
                );
            }

            public void AddRequest(HttpRequest request)
            {
                var headersLength = request.Headers.Sum(x => x.Key.Length + x.Value.Sum(x => x.Length) + 2);
                var methodLength = request.Method.Length + 1 + request.Uri.AbsoluteUri.Length;
                var contentLength = request.Content?.Length ?? 0;
                var requestLength = methodLength + headersLength + contentLength;

                unchecked
                {
                    Interlocked.Increment(ref _sent);
                    Interlocked.Add(ref _sentBytes, requestLength);
                }
            }

            public void AddSuccess(HttpResponse response, long timeElapsed)
            {
                var headersLength = response.Headers.Sum(x => x.Key.Length + x.Value.Sum(x => x.Length) + 2);
                var contentLength = response.Content?.Length ?? 0;
                var responseLength = headersLength + contentLength;

                unchecked
                {
                    Interlocked.Increment(ref _success);
                    Interlocked.Add(ref _receivedBytes, responseLength);
                    Interlocked.Add(ref _timeElapsed, timeElapsed);
                }
            }

            public void AddError(Exception ex, long timeElapsed)
            {
                FailType type;

                if (ex is HttpRequestException &&
                    (ex.InnerException != null ||
                        ex.Message.Contains("proxy", StringComparison.OrdinalIgnoreCase) ||
                        ex.Message.Contains("The server shut down the connection", StringComparison.OrdinalIgnoreCase) ||
                        ex.Message.Contains("An HTTP/2 connection could not be established because the server did not complete the HTTP/2 handshake", StringComparison.OrdinalIgnoreCase)))
                {
                    type = FailType.Proxy;
                }
                else if (ex is TaskCanceledException)
                {
                    type = FailType.Timeout;
                }
                else
                {
                    type = FailType.Other;
                }

                unchecked
                {
                    _failed.AddOrUpdate(type, 1, (x, c) => c + 1);
                    Interlocked.Add(ref _timeElapsed, timeElapsed);
                }
            }
        }

        public StatisticsHttpRequestHandler(IHttpRequestHandler handler, Storage storage)
        {
            _handler = handler;
            _storage = storage;
        }

        public HttpRequestHandlerOptions Options => _handler.Options;

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await _handler.SendAsync(request, cancellationToken);

                stopwatch.Stop();

                _storage.AddRequest(request);
                _storage.AddSuccess(response, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException && cancellationToken.IsCancellationRequested)
                    throw;

                stopwatch.Stop();

                _storage.AddRequest(request);
                _storage.AddError(ex, stopwatch.ElapsedMilliseconds);

                throw;
            }
        }

        public IHttpRequestHandler Clone()
        {
            return new StatisticsHttpRequestHandler(_handler.Clone(), _storage);
        }

        public void Dispose()
        {
            _handler.Dispose();
        }
    }
}
