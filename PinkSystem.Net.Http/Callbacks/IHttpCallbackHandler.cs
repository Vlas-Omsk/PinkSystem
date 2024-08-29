namespace PinkSystem.Net.Http.Callbacks
{
    public interface IHttpCallbackHandler
    {
        IHttpCallbackReceiver CreateReceiver(string path);
    }
}
