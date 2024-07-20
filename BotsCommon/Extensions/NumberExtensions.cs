namespace BotsCommon
{
    public static class NumberExtensions
    {
        private readonly static string[] _byteSuffixes = { "B", "KB", "MB", "GB", "TB" };

        public static string FormatBytes(this int self)
        {
            return FormatBytes((long)self);
        }

        public static string FormatBytes(this long self)
        {
            double dblSByte = self;
            int i;

            for (i = 0; i < _byteSuffixes.Length && self >= 1024; i++, self /= 1024)
                dblSByte = self / 1024.0;

            return string.Format("{0:0.##} {1}", dblSByte, _byteSuffixes[i]);
        }

        public static string FormatPercents(this double self, string format = "0.00")
        {
            return (self * 100).ToString(format) + "%";
        }
    }
}
