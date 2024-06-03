namespace BotsCommon.Net.Http
{
    public sealed record HttpRequestHandlerOptions
    {
        public Proxy? Proxy { get; set; }
        public bool ValidateSsl { get; set; } = true;
    }
}
