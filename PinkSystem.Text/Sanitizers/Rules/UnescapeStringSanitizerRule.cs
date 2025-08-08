using System.IO;
using PinkSystem.Text.Sanitizing.Rules;

namespace PinkSystem.Text.Sanitizers.Rules
{
    public sealed class UnescapeStringSanitizerRule : IStringSanitizerRule
    {
        private readonly IEscapeSequenceEncoder _map;

        public UnescapeStringSanitizerRule(IEscapeSequenceEncoder map)
        {
            _map = map;
        }

        public bool TrySanitize(BufferedTextReader reader, TextWriter writer)
        {
            var chars = reader.PeekSpanUnsafe(2);

            if (chars.Length < 2 || chars[0] != '\\')
                return false;

            reader.Read();

            if (_map.TryDecode(reader, writer))
                return true;

            writer.Write(chars[1]);
            reader.Read();
            return true;
        }
    }
}
