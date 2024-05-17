namespace BotsCommon.Net.Http
{
    public sealed class HttpRequestHandlerOptions
    {
        public Proxy? Proxy { get; set; }
        public bool ValidateSsl { get; set; } = true;
    }
}
