using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http.Callbacks
{
    public interface IHttpCallbackHandler
    {
        bool TryHandle(HttpRequest request);
    }

    public sealed record HttpCallbackHandler<T> : IHttpCallbackHandler
    {
        private readonly Func<HttpRequest, HttpCallbackHandlerResponse<T>> _func;
        private readonly SemaphoreSlim _event = new(0, 1);
        private T? _result;
        private Exception? _exception;

        public HttpCallbackHandler(Func<HttpRequest, HttpCallbackHandlerResponse<T>> func)
        {
            _func = func;
        }

        public bool TryHandle(HttpRequest request)
        {
            HttpCallbackHandlerResponse<T> response;

            try
            {
                response = _func(request);
            }
            catch (Exception ex)
            {
                _exception = ex;

                _event.Release();

                return true;
            }

            if (response.Handled)
            {
                _result = response.Value;

                _event.Release();

                return true;
            }

            return false;
        }

        public async Task<T> Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (!await _event.WaitAsync(timeout, cancellationToken))
                throw new TimeoutException("Timeout when waiting message");

            cancellationToken.ThrowIfCancellationRequested();

            if (_exception != null)
                throw new Exception("Error when handling callback", _exception);

            return _result!;
        }
    };

    public readonly struct HttpCallbackHandlerResponse<T>
    {
        public HttpCallbackHandlerResponse(bool handled, T? value)
        {
            Handled = handled;
            Value = value;
        }

        public bool Handled { get; }
        public T? Value { get; }
    }
}
