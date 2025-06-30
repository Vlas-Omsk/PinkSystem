using PinkSystem.Text.Sanitizing;

namespace PinkSystem.Text
{
    public static class StringExtension
    {
        public static string Excape(this string self)
        {
            return StringSanitizer.DefaultEscaper.Sanitize(self);
        }

        public static string Unescape(this string self)
        {
            return StringSanitizer.DefaultUnescaper.Sanitize(self);
        }

        public static string ToUnicode(this string self)
        {
            return StringSanitizer.DefaultUnicoder.Sanitize(self);
        }
    }
}
