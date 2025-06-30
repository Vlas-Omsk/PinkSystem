using System.IO;

namespace PinkSystem.Text.Sanitizing.Rules
{
    public interface IStringSanitizerRule
    {
        bool TrySanitize(BufferedTextReader reader, TextWriter writer);
    }
}
