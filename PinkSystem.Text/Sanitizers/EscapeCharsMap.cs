using System;
using System.Collections.Generic;
using System.IO;

namespace PinkSystem.Text.Sanitizers
{
    public sealed class EscapeCharsMap : IEscapeCharsMap
    {
        private readonly Dictionary<char, string> _unescapedEscapedMap = new();
        private readonly EscapedUnescapedNode _escapedUnescapedNode = new();
        private readonly List<char> _unicodeChars = new();
        private int _maxSequenceLength;

        private sealed class EscapedUnescapedNode
        {
            public char? @Char { get; set; }
            public Dictionary<char, EscapedUnescapedNode> Next { get; } = new();
        }

        public void Add(char unescaped, string escaped)
        {
            _unescapedEscapedMap.Add(unescaped, escaped);

            var node = _escapedUnescapedNode;

            foreach (var escapedChar in escaped)
            {
                if (!node.Next.TryGetValue(escapedChar, out var nextNode))
                    node.Next.Add(escapedChar, nextNode = new());

                node = nextNode;
            }

            if (node.Char.HasValue)
                throw new ArgumentException("An item with the same key has already been added", "key");

            node.Char = unescaped;

            _maxSequenceLength = Math.Max(_maxSequenceLength, escaped.Length);
        }

        public void AddUnicode(char unescaped)
        {
            _unicodeChars.Add(unescaped);
        }

        public bool TryEscape(BufferedTextReader reader, TextWriter writer)
        {
            var chars = reader.PeekSpanUnsafe(1);

            if (_unicodeChars.Contains(chars[0]))
            {
                writer.Write($"u{Convert.ToString(chars[0], 16).PadLeft(4, '0').ToUpper()}");
                reader.Read();
                return true;
            }

            if (_unescapedEscapedMap.TryGetValue(chars[0], out var escaped))
            {
                writer.Write(escaped);
                reader.Read();
                return true;
            }

            return false;
        }

        public bool TryUnescape(BufferedTextReader reader, TextWriter writer)
        {
            if (_maxSequenceLength == 0)
                return false;

            var chars = reader.PeekSpanUnsafe(1);

            if (chars[0] == 'u')
            {
                reader.Read();

                var value = UnescapeUnicode(reader);

                writer.Write(value);
                return true;
            }

            var node = _escapedUnescapedNode;
            var sequenceLength = 0;

            for (; sequenceLength < _maxSequenceLength; sequenceLength++)
            {
                if (chars.Length != sequenceLength + 1)
                    break;

                if (!node.Next.TryGetValue(chars[sequenceLength], out var nextNode))
                    break;

                node = nextNode;
                chars = reader.PeekSpanUnsafe(chars.Length + 1);
            }

            if (!node.Char.HasValue)
                return false;

            for (var i = 0; i < sequenceLength; i++)
                reader.Read();

            writer.Write(node.Char.Value);
            return true;
        }

        private static char UnescapeUnicode(BufferedTextReader reader)
        {
            var value = 0;

            for (var i = 0; i < 4; i++)
            {
                int c = reader.Read();

                if (c == -1)
                    throw new Exception("Unexpected end of unicode sequence");

                var digit =
                    c >= '0' && c <= '9' ? c - '0' :
                    c >= 'A' && c <= 'F' ? c - 'A' + 10 :
                    c >= 'a' && c <= 'f' ? c - 'a' + 10 :
                    throw new Exception("Invalid hex digit: " + (char)c);

                value = value << 4 | digit;
            }

            return (char)value;
        }

        public static EscapeCharsMap CreateDefault()
        {
            var map = new EscapeCharsMap();

            map.Add('\b', "b");
            map.Add('\a', "a");
            map.Add('\f', "f");
            map.Add('\n', "n");
            map.Add('\r', "r");
            map.Add('\t', "t");
            map.Add('\0', "0");
            map.Add('\"', "\"");
            map.Add('\\', "\\");
            map.Add('/', "/");

            return map;
        }
    }
}
