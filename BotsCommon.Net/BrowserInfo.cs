namespace BotsCommon.Net
{
    public sealed record BrowserInfo(
        string UserAgent,
        string? SecChUa
    )
    {
        public override string ToString()
        {
            if (SecChUa == null)
                return UserAgent;

            return $"{UserAgent}|{SecChUa}";
        }
    }
}
