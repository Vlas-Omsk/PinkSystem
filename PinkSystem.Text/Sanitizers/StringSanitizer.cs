using System.Collections.Immutable;
using System.IO;
using PinkSystem.Text.Sanitizers;
using PinkSystem.Text.Sanitizers.Rules;
using PinkSystem.Text.Sanitizing.Rules;

namespace PinkSystem.Text.Sanitizing
{
    public sealed class StringSanitizer
    {
        private static readonly EscapeCharsMap _defaultEscapeCharsMap = EscapeCharsMap.CreateDefault();
        private readonly ImmutableArray<IStringSanitizerRule> _rules;

        public StringSanitizer(ImmutableArray<IStringSanitizerRule> rules)
        {
            _rules = rules;
        }

        public static StringSanitizer DefaultEscaper { get; } = new([
            new EscapeStringSanitizerRule(_defaultEscapeCharsMap)
        ]);
        public static StringSanitizer DefaultUnescaper { get; } = new([
            new UnescapeStringSanitizerRule(_defaultEscapeCharsMap)
        ]);
        public static StringSanitizer DefaultUnicoder { get; } = new([
            new UnicodeStringSanitizerRule()
        ]);

        public string Sanitize(string str)
        {
            using (var reader = new StringReader(str))
            using (var writer = new StringWriter())
            {
                Sanitize(reader, writer);

                return writer.ToString();
            }
        }

        public void Sanitize(TextReader reader, TextWriter writer)
        {
            var bufferedReader = new BufferedTextReader(reader);

            while (bufferedReader.Peek() != -1)
            {
                var ruleApplied = false;

                foreach (var rule in _rules)
                {
                    if (!rule.TrySanitize(bufferedReader, writer))
                        continue;

                    ruleApplied = true;
                    break;
                }

                if (ruleApplied)
                    continue;

                writer.Write((char)bufferedReader.Read());
            }
        }
    }
}
