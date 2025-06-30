using System.IO;
using PinkSystem.Text.Sanitizing.Rules;

namespace PinkSystem.Text.Sanitizers.Rules
{
    public sealed class EscapeStringSanitizerRule : IStringSanitizerRule
    {
        private readonly IEscapeCharsMap _map;

        public EscapeStringSanitizerRule(IEscapeCharsMap map)
        {
            _map = map;
        }

        public bool TrySanitize(BufferedTextReader reader, TextWriter writer)
        {
            return _map.TryEscape(reader, writer);
        }
    }
}
