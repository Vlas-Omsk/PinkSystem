using System.IO;

namespace PinkSystem.Text.Sanitizers
{
    public interface IEscapeCharsMap
    {
        bool TryEscape(BufferedTextReader reader, TextWriter writer);
        bool TryUnescape(BufferedTextReader reader, TextWriter writer);
    }
}
