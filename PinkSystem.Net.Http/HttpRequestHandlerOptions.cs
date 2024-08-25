namespace PinkSystem.Net.Http
{
    public record HttpRequestHandlerOptions
    {
        public Proxy? Proxy { get; set; }
        public bool ValidateSsl { get; set; } = true;
    }
}
