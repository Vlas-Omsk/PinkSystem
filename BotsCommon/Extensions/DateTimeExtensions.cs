using System;

namespace BotsCommon
{
    public static class DateTimeExtensions
    {
        public static string ToISO8601String(this DateTime self)
        {
            return self.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
        }
    }
}
