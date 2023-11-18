namespace BotsCommon
{
    public enum UnixTimestampPercision
    {
        Ticks,
        Milliseconds,
        Seconds
    }

    public sealed class UnixTimestamp
    {
        public UnixTimestamp(TimeSpan timeSpan, UnixTimestampPercision percision)
        {
            DateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Add(timeSpan);
            TimeSpan = timeSpan;
            Percision = percision;
        }

        public DateTime DateTime { get; }
        public TimeSpan TimeSpan { get; }
        public UnixTimestampPercision Percision { get; }
        public long Ticks => TimeSpan.Ticks;
        public double Milliseconds => TimeSpan.TotalMilliseconds;
        public long MillisecondsLong => (long)Math.Round(TimeSpan.TotalMilliseconds);
        public double Seconds => TimeSpan.TotalSeconds;
        public long SecondsLong => (long)Math.Round(TimeSpan.TotalSeconds);

        public static UnixTimestamp operator -(UnixTimestamp left, UnixTimestamp right)
        {
            return GetMaxPercision(left.Percision, right.Percision) switch
            {
                UnixTimestampPercision.Ticks => FromTicks(left.Ticks - right.Ticks),
                UnixTimestampPercision.Milliseconds => FromMilliseconds(left.MillisecondsLong - right.MillisecondsLong),
                UnixTimestampPercision.Seconds => FromSeconds(left.SecondsLong - right.SecondsLong),
                _ => throw new NotSupportedException(),
            };
        }

        public static UnixTimestamp operator +(UnixTimestamp left, UnixTimestamp right)
        {
            return GetMaxPercision(left.Percision, right.Percision) switch
            {
                UnixTimestampPercision.Ticks => FromTicks(left.Ticks + right.Ticks),
                UnixTimestampPercision.Milliseconds => FromMilliseconds(left.MillisecondsLong + right.MillisecondsLong),
                UnixTimestampPercision.Seconds => FromSeconds(left.SecondsLong + right.SecondsLong),
                _ => throw new NotSupportedException(),
            };
        }

        private static UnixTimestampPercision GetMaxPercision(params UnixTimestampPercision[] percisions)
        {
            return percisions.Max();
        }

        public static UnixTimestamp Now => FromDateTime(DateTime.UtcNow, UnixTimestampPercision.Ticks);

        public static UnixTimestamp FromDateTime(DateTime dateTime, UnixTimestampPercision percision)
        {
            return new UnixTimestamp(
                dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                percision
            );
        }

        public static UnixTimestamp FromTicks(long ticks)
        {
            return new UnixTimestamp(
                TimeSpan.FromTicks(ticks),
                UnixTimestampPercision.Ticks
            );
        }

        public static UnixTimestamp FromMilliseconds(double milliseconds)
        {
            return new UnixTimestamp(
                TimeSpan.FromMilliseconds(milliseconds),
                UnixTimestampPercision.Milliseconds
            );
        }

        public static UnixTimestamp FromSeconds(double seconds)
        {
            return new UnixTimestamp(
                TimeSpan.FromSeconds(seconds),
                UnixTimestampPercision.Seconds
            );
        }
    }
}
