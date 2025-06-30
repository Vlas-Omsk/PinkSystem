using System.IO;

namespace PinkSystem.Text.Sanitizers
{
    public interface IEscapeSequenceEncoder
    {
        bool TryEncode(BufferedTextReader reader, TextWriter writer);
        bool TryDecode(BufferedTextReader reader, TextWriter writer);
    }
}
