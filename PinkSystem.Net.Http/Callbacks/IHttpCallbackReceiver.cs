using System;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem.Net.Http.Callbacks
{
    public interface IHttpCallbackReceiver : IDisposable
    {
        Uri ExternalUri { get; }

        Task<HttpRequest> Receive(TimeSpan timeout, CancellationToken cancellationToken);
    }
}
