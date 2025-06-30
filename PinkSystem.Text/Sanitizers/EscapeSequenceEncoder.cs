using System;
using System.Collections.Generic;
using System.IO;

namespace PinkSystem.Text.Sanitizers
{
    public sealed class EscapeSequenceEncoder : IEscapeSequenceEncoder
    {
        private readonly Dictionary<char, string> _encodeMap = new();
        private readonly DecodeMapNode _decodeMap = new();
        private readonly List<char> _unicodeChars = new();
        private int _maxSequenceLength;

        private sealed class DecodeMapNode
        {
            public char? Value { get; set; }
            public Dictionary<char, DecodeMapNode> NextMap { get; } = new();
        }

        public void Add(char decoded, string encoded)
        {
            _encodeMap.Add(decoded, encoded);

            AddDecodeNode(encoded, decoded);

            _maxSequenceLength = Math.Max(_maxSequenceLength, encoded.Length);
        }

        public void AddUnicode(char decoded)
        {
            _unicodeChars.Add(decoded);
        }

        public bool TryEncode(BufferedTextReader reader, TextWriter writer)
        {
            var chars = reader.PeekSpanUnsafe(1);

            if (chars.Length != 1)
                return false;

            if (_unicodeChars.Contains(chars[0]))
            {
                writer.Write($"u{Convert.ToString(chars[0], 16).PadLeft(4, '0').ToUpper()}");
                reader.Read();
                return true;
            }

            if (_encodeMap.TryGetValue(chars[0], out var escaped))
            {
                writer.Write(escaped);
                reader.Read();
                return true;
            }

            return false;
        }

        public bool TryDecode(BufferedTextReader reader, TextWriter writer)
        {
            var chars = reader.PeekSpanUnsafe(1);

            if (chars.Length != 1)
                return false;

            if (chars[0] == 'u')
            {
                reader.Read();

                var value = DecodeUnicode(reader);

                writer.Write(value);
                return true;
            }

            var node = FindDecodeNodeFromReader(reader, out var sequenceLength);

            if (node.Value.HasValue)
            {
                for (var i = 0; i < sequenceLength; i++)
                    reader.Read();

                writer.Write(node.Value.Value);
                return true;
            }

            return false;
        }

        private void AddDecodeNode(string encoded, char decoded)
        {
            var node = _decodeMap;

            foreach (var encodedChar in encoded)
            {
                if (!node.NextMap.TryGetValue(encodedChar, out var nextNode))
                    node.NextMap.Add(encodedChar, nextNode = new());

                node = nextNode;
            }

            if (node.Value.HasValue)
                throw new ArgumentException("An item with the same key has already been added", "key");

            node.Value = decoded;
        }

        private DecodeMapNode FindDecodeNodeFromReader(BufferedTextReader reader, out int sequenceLength)
        {
            sequenceLength = 0;

            var node = _decodeMap;

            for (; sequenceLength < _maxSequenceLength; sequenceLength++)
            {
                var chars = reader.PeekSpanUnsafe(sequenceLength + 1);

                if (chars.Length != sequenceLength + 1)
                    break;

                if (!node.NextMap.TryGetValue(chars[sequenceLength], out var nextNode))
                    break;

                node = nextNode;
            }

            return node;
        }

        private static char DecodeUnicode(BufferedTextReader reader)
        {
            var value = 0;

            for (var i = 0; i < 4; i++)
            {
                var @char = reader.Read();

                if (@char == -1)
                    throw new Exception("Unexpected end of unicode sequence");

                var digit =
                    @char >= '0' && @char <= '9' ? @char - '0' :
                    @char >= 'A' && @char <= 'F' ? @char - 'A' + 10 :
                    @char >= 'a' && @char <= 'f' ? @char - 'a' + 10 :
                    throw new Exception("Invalid hex digit: " + (char)@char);

                value = value << 4 | digit;
            }

            return (char)value;
        }

        public static EscapeSequenceEncoder CreateDefault()
        {
            var map = new EscapeSequenceEncoder();

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
