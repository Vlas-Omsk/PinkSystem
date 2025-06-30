using System;
using System.IO;

namespace PinkSystem
{
    public static class StringExtension
    {
        public static string Repeat(this string self, int count)
        {
            using (var writer = new StringWriter())
            {
                Repeat(self, count, writer);

                return writer.ToString();
            }
        }

        public static void Repeat(this string self, int count, TextWriter writer)
        {
            for (var i = 0; i < count; i++)
                writer.Write(self);
        }

        public static string MakeIndent(this string self, int indentSize)
        {
            var lines = self.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            var indent = new string(' ', indentSize);

            return indent + string.Join(
                Environment.NewLine + indent,
                lines
            );
        }
    }
}
