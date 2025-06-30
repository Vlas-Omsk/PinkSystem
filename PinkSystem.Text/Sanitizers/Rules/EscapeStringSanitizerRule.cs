using System;
using System.IO;
using System.Text;
using PinkSystem.Text.Sanitizing.Rules;

namespace PinkSystem.Text.Sanitizers.Rules
{
    public sealed class EscapeStringSanitizerRule : IStringSanitizerRule
    {
        private readonly IEscapeSequenceEncoder _encoder;

        private sealed class PrefixedTextWriter : TextWriter
        {
            private readonly TextWriter _writer;
            private bool _prefixWrited = false;

            public PrefixedTextWriter(TextWriter writer)
            {
                _writer = writer;
            }

            public override Encoding Encoding => _writer.Encoding;
            public override string NewLine
            {
                get => _writer.NewLine;
                set => _writer.NewLine = value;
            }
            public override IFormatProvider FormatProvider => _writer.FormatProvider;

            public override void Write(char value)
            {
                if (!_prefixWrited)
                {
                    _writer.Write('\\');
                    
                    _prefixWrited = true;
                }

                _writer.Write(value);
            }

            public override void Write(char[] buffer, int index, int count)
            {
                if (!_prefixWrited)
                {
                    _writer.Write('\\');

                    _prefixWrited = true;
                }

                _writer.Write(buffer, index, count);
            }
        }

        public EscapeStringSanitizerRule(IEscapeSequenceEncoder encoder)
        {
            _encoder = encoder;
        }

        public bool TrySanitize(BufferedTextReader reader, TextWriter writer)
        {
            return _encoder.TryEncode(
                reader,
                new PrefixedTextWriter(writer)
            );
        }
    }
}
