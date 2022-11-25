using System;

namespace BotsCommon
{
    public static class TimeConverter
    {
        public static DateTime FromUnixTimestamp(long unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime).ToLocalTime();
        }
    }
}
