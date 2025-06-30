using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using PinkSystem.Text.Sanitizing.Rules;

namespace PinkSystem.Text.Sanitizers.Rules
{
    public sealed class EscapeStringSanitizerRule : IStringSanitizerRule
    {
        private readonly ImmutableArray<char> _chars;
        private readonly ImmutableArray<char> _unicodeChars;

        public EscapeStringSanitizerRule(IEnumerable<char> chars, IEnumerable<char> unicodeChars)
        {
            _chars = chars.ToImmutableArray();
            _unicodeChars = unicodeChars.ToImmutableArray();
        }

        public bool TrySanitize(BufferedTextReader str, TextWriter writer)
        {
            var chars = str.PeekSpanUnsafe(1);

            switch (chars[0])
            {
                case '\b':
                    str.Read();
                    writer.Write("\\b");
                    return true;
                case '\a':
                    str.Read();
                    writer.Write("\\a");
                    return true;
                case '\f':
                    str.Read();
                    writer.Write("\\f");
                    return true;
                case '\n':
                    str.Read();
                    writer.Write("\\n");
                    return true;
                case '\r':
                    str.Read();
                    writer.Write("\\r");
                    return true;
                case '\t':
                    str.Read();
                    writer.Write("\\t");
                    return true;
                case '\0':
                    str.Read();
                    writer.Write("\\0");
                    return true;
                case '\"':
                    str.Read();
                    writer.Write("\\\"");
                    return true;
                case '\\':
                    str.Read();
                    writer.Write("\\\\");
                    return true;
                default:
                    if (_chars.Contains(chars[0]))
                    {
                        writer.Write($"\\{chars[0]}");
                        str.Read();
                        return true;
                    }
                    if (_unicodeChars.Contains(chars[0]))
                    {
                        writer.Write($"\\u{Convert.ToString(chars[0], 16).PadLeft(4, '0').ToUpper()}");
                        str.Read();
                        return true;
                    }
                    return false;
            }
        }
    }
}
