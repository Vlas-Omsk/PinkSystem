using System;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Handlers
{
    public interface IHttpRequestHandler : IDisposable
    {
        Proxy? Proxy { get; set; }
        bool ValidateSsl { get; set; }
        TimeSpan Timeout { get; set; }

        Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken);

        IHttpRequestHandler Clone();
    }
}
