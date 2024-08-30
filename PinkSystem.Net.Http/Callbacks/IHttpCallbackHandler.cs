using System.Diagnostics.CodeAnalysis;

namespace PinkSystem.Net.Http.Callbacks
{
    public interface IHttpCallbackHandler
    {
        bool TryCreateReceiver(string path, [NotNullWhen(true)] out IHttpCallbackReceiver? receiver);
    }
}
