using System;
using System.Buffers;
using System.IO;
using System.Linq;
using PinkSystem.Text.Sanitizing.Rules;

namespace PinkSystem.Text.Sanitizers.Rules
{
    public sealed class UnescapeStringSanitizerRule : IStringSanitizerRule
    {
        public bool TrySanitize(BufferedTextReader reader, TextWriter writer)
        {
            var chars = reader.PeekSpanUnsafe(1);

            if (chars[0] != '\\')
                return false;

            // Max size of buffer 4 due to reading 4 chars of unicode escape sequence
            var buffer = ArrayPool<char>.Shared.Rent(4);

            try
            {
                var readedSize = reader.Read(buffer, 0, 2);

                if (readedSize == 1)
                    return false;

                switch (buffer[1])
                {
                    case 'b':
                        writer.Write('\b');
                        return true;
                    case 'a':
                        writer.Write('\a');
                        return true;
                    case 'f':
                        writer.Write('\f');
                        return true;
                    case 'n':
                        writer.Write('\n');
                        return true;
                    case 'r':
                        writer.Write('\r');
                        return true;
                    case 't':
                        writer.Write('\t');
                        return true;
                    case '0':
                        writer.Write("\0");
                        return true;
                    case 'u':
                        readedSize = reader.Read(buffer, 0, 4);

                        if (readedSize != 4)
                            throw new FormatException($"Unicode value must be hexadecimal and 4 characters long");

                        writer.Write(HexArrayToChar(buffer));
                        return true;
                    case '"':
                        writer.Write('\"');
                        return true;
                    case '\\':
                        writer.Write('\\');
                        return true;
                    case '/':
                        writer.Write('/');
                        return true;
                    default:
                        writer.Write(buffer[2]);
                        return true;
                }
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        private static char HexArrayToChar(ReadOnlySpan<char> hexArray)
        {
            if (hexArray.Length > 4)
                throw new ArgumentException("Maximum size of unicode hex array is 4");

            var value = 0;

            for (var i = 0; i < 4; i++)
            {
                int c = hexArray[i];
                var digit =
                    (c >= '0' && c <= '9') ? (c - '0') :
                    (c >= 'A' && c <= 'F') ? (c - 'A' + 10) :
                    (c >= 'a' && c <= 'f') ? (c - 'a' + 10) :
                    throw new ArgumentException("Invalid hex digit: " + (char)c);

                value = (value << 4) | digit;
            }

            return (char)value;
        }
    }
}
