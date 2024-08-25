using System;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers
{
    public interface IHttpRequestHandler : IDisposable
    {
        HttpRequestHandlerOptions Options { get; }

        Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken);

        IHttpRequestHandler Clone();
    }
}
