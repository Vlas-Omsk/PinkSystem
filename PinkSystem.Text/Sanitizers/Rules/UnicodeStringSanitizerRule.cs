using System.IO;
using PinkSystem.Text.Sanitizing.Rules;

namespace PinkSystem.Text.Sanitizers.Rules
{
    public sealed class UnicodeStringSanitizerRule : IStringSanitizerRule
    {
        public bool TrySanitize(BufferedTextReader reader, TextWriter writer)
        {
            var readedChar = (char)reader.Read();

            writer.Write(readedChar.ToUnicode());
            return true;
        }
    }
}
