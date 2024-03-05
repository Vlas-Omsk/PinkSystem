using System;

namespace BotsCommon
{
    public static class CharExtensions
    {
        public static string Repeat(this char self, int count)
        {
            return new string(self, count);
        }

        public static string ToUnicode(this char self)
        {
            var val = Convert.ToString(self, 16);

            return "\\u" + new string('0', 4 - val.Length) + val;
        }
    }
}
