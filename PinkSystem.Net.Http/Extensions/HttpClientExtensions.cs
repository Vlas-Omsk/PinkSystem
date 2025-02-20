using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> SendWithExceptionWrappingAsync(this HttpClient self, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await self.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex.InnerException is TimeoutException)
            {
                throw new TimeoutException(ex.Message, ex.InnerException?.InnerException);
            }
            catch (Exception ex) when (ex.CheckAny(ex =>
                (ex is HttpRequestException &&
                    (ex.InnerException != null ||
                        ex.Message.Contains("The server shut down the connection", StringComparison.OrdinalIgnoreCase) ||
                        ex.Message.Contains("An HTTP/2 connection could not be established because the server did not complete the HTTP/2 handshake", StringComparison.OrdinalIgnoreCase))) ||
                (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested)
            ))
            {
                throw new HttpConnectionRefusedException("Http connection refused", ex);
            }
            catch (Exception ex) when (ex.CheckAny(ex =>
                ex is HttpRequestException &&
                    ex.Message.Contains("proxy", StringComparison.OrdinalIgnoreCase)
            ))
            {
                throw new ProxyConnectionRefusedException("Proxy connection refused", ex);
            }
        }
    }
}
