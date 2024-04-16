using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon.Net.Http.Callbacks
{
    public interface IHttpCallbackReceiver
    {
        bool IsListening { get; }

        void AddHandler(IHttpCallbackHandler handler);
        Uri GetUri(string path);
        Task Start(CancellationToken cancellationToken);
    }
}