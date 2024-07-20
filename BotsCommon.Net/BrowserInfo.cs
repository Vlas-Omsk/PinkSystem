namespace BotsCommon.Net
{
    public sealed record BrowserInfo(
        string UserAgent,
        string? SecChUa,
        Ja3Fingerprint? Fingerprint
    )
    {
        public override string ToString()
        {
            if (SecChUa == null)
                return UserAgent;

            return $"{UserAgent}|{SecChUa}";
        }

        public static BrowserInfo Parse(string str)
        {
            var parts = str.Split('|');

            if (parts.Length == 2)
                return new(parts[0], parts[1], null);

            if (parts.Length == 3)
                return new(parts[0], parts[1], Ja3Fingerprint.Parse(parts[2]));

            return new(parts[0], null, null);
        }
    }
}
