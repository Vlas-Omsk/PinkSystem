namespace BotsCommon
{
    public sealed class UnixTimestamp
    {
        public UnixTimestamp(double unixTime) : this(
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(unixTime)
                .ToLocalTime()
        )
        {
        }

        public UnixTimestamp(DateTime dateTime)
        {
            DateTime = dateTime;
            TimeSpan = dateTime
                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            if (TimeSpan.Ticks < 0)
                TimeSpan = TimeSpan.Zero;
        }

        public DateTime DateTime { get; }
        public TimeSpan TimeSpan { get; }
        public double Seconds => TimeSpan.TotalSeconds;
        public double Milliseconds => TimeSpan.TotalMilliseconds;

        public static UnixTimestamp Now => new UnixTimestamp(DateTime.Now);
        public static UnixTimestamp UtcNow => new UnixTimestamp(DateTime.UtcNow);
    }
}
