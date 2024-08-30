namespace PinkSystem.Net.Http.Callbacks
{
    public interface IHttpCallbackHandler
    {
        bool TryCreateReceiver(string path, out IHttpCallbackReceiver receiver);
    }
}
