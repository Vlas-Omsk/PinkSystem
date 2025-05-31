using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers
{
    public enum FailType
    {
        Timeout,
        Proxy,
        Other
    }

    public sealed class StatisticsStorage
    {
        private long _sent = 0;
        private long _successRequestsAmount = 0;
        private readonly ConcurrentDictionary<FailType, long> _failedRequestsAmount = new();
        private long _timeElapsed = 0;
        private long _sentBytes = 0;
        private long _receivedBytes = 0;

        public long SentRequests => _sent;
        public long SuccessResponsesAmount => _successRequestsAmount;
        public IReadOnlyDictionary<FailType, long> FailedResponsesAmount => _failedRequestsAmount;
        public long TimeElapsed => _timeElapsed;
        public long SentBytes => _sentBytes;
        public long ReceivedBytes => _receivedBytes;

        public void AddNewRequest(HttpRequest request)
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

        public void AddSuccessResponse(HttpResponse response, long timeElapsed)
        {
            var headersLength = response.Headers.Sum(x => x.Key.Length + x.Value.Sum(x => x.Length) + 2);
            var contentLength = response.Content?.Length ?? 0;
            var responseLength = headersLength + contentLength;

            unchecked
            {
                Interlocked.Increment(ref _successRequestsAmount);
                Interlocked.Add(ref _receivedBytes, responseLength);
                Interlocked.Add(ref _timeElapsed, timeElapsed);
            }
        }

        public void AddErrorResponse(Exception ex, long timeElapsed)
        {
            FailType type;

            if (ex is ProxyConnectionRefusedException)
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
                _failedRequestsAmount.AddOrUpdate(type, 1, (x, c) => c + 1);
                Interlocked.Add(ref _timeElapsed, timeElapsed);
            }
        }
    }

    public sealed class StatisticsHttpRequestHandlerFactory : IHttpRequestHandlerFactory
    {
        private readonly IHttpRequestHandlerFactory _handlerFactory;
        private readonly StatisticsStorage _storage;

        private sealed class StatisticsHttpRequestHandler : ExtensionHttpRequestHandler
        {
            private readonly StatisticsStorage _storage;

            public StatisticsHttpRequestHandler(IHttpRequestHandler handler, StatisticsStorage storage) : base(handler)
            {
                _storage = storage;
            }

            public override async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken)
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    var response = await Handler.SendAsync(request, cancellationToken);

                    stopwatch.Stop();

                    _storage.AddNewRequest(request);
                    _storage.AddSuccessResponse(response, stopwatch.ElapsedMilliseconds);

                    return response;
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException && cancellationToken.IsCancellationRequested)
                        throw;

                    stopwatch.Stop();

                    _storage.AddNewRequest(request);
                    _storage.AddErrorResponse(ex, stopwatch.ElapsedMilliseconds);

                    throw;
                }
            }
        }

        public StatisticsHttpRequestHandlerFactory(IHttpRequestHandlerFactory handlerFactory, StatisticsStorage storage)
        {
            _handlerFactory = handlerFactory;
            _storage = storage;
        }

        public IHttpRequestHandler Create(IHttpRequestHandlerOptions? options)
        {
            return new StatisticsHttpRequestHandler(
                _handlerFactory.Create(options),
                _storage
            );
        }
    }
}
