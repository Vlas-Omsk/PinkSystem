using System;
using System.Linq;

namespace BotsCommon
{
    public enum UnixTimestampPercision
    {
        Ticks,
        Milliseconds,
        Seconds,
        Minutes,
        Hours,
        Days
    }

    public readonly struct UnixTimestamp
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
        public double Minutes => TimeSpan.TotalMinutes;
        public long MinutesLong => (long)Math.Round(TimeSpan.TotalMinutes);
        public double Hours => TimeSpan.TotalHours;
        public long HoursLong => (long)Math.Round(TimeSpan.TotalHours);
        public double Days => TimeSpan.TotalDays;
        public long DaysLong => (long)Math.Round(TimeSpan.TotalDays);

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (obj is UnixTimestamp unixTimestamp)
                return Ticks == unixTimestamp.Ticks;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return DateTime.GetHashCode();
        }

        public static UnixTimestamp operator -(UnixTimestamp left, UnixTimestamp right)
        {
            return GetMaxPercision(left.Percision, right.Percision) switch
            {
                UnixTimestampPercision.Ticks => FromTicks(left.Ticks - right.Ticks),
                UnixTimestampPercision.Milliseconds => FromMilliseconds(left.MillisecondsLong - right.MillisecondsLong),
                UnixTimestampPercision.Seconds => FromSeconds(left.SecondsLong - right.SecondsLong),
                UnixTimestampPercision.Minutes => FromMinutes(left.MinutesLong - right.MinutesLong),
                UnixTimestampPercision.Hours => FromHours(left.HoursLong - right.HoursLong),
                UnixTimestampPercision.Days => FromDays(left.DaysLong - right.DaysLong),
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
                UnixTimestampPercision.Minutes => FromMinutes(left.MinutesLong + right.MinutesLong),
                UnixTimestampPercision.Hours => FromHours(left.HoursLong + right.HoursLong),
                UnixTimestampPercision.Days => FromDays(left.DaysLong + right.DaysLong),
                _ => throw new NotSupportedException(),
            };
        }

        public static bool operator >(UnixTimestamp? left, UnixTimestamp? right)
        {
            return left?.Ticks > right?.Ticks;
        }

        public static bool operator <(UnixTimestamp? left, UnixTimestamp? right)
        {
            return left?.Ticks < right?.Ticks;
        }

        public static bool operator ==(UnixTimestamp? left, UnixTimestamp? right)
        {
            if (object.Equals(left, null) || object.Equals(right, null))
                return object.Equals(left, right);

            return left.Equals(right);
        }

        public static bool operator !=(UnixTimestamp? left, UnixTimestamp? right)
        {
            return !(left == right);
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

        public static UnixTimestamp FromMinutes(double minutes)
        {
            return new UnixTimestamp(
                TimeSpan.FromMinutes(minutes),
                UnixTimestampPercision.Minutes
            );
        }

        public static UnixTimestamp FromHours(double hours)
        {
            return new UnixTimestamp(
                TimeSpan.FromHours(hours),
                UnixTimestampPercision.Hours
            );
        }

        public static UnixTimestamp FromDays(double days)
        {
            return new UnixTimestamp(
                TimeSpan.FromDays(days),
                UnixTimestampPercision.Days
            );
        }
    }
}
