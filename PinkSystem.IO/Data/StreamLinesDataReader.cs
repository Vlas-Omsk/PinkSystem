using System.IO;

namespace PinkSystem.IO.Data
{
    public sealed class StreamLinesDataReader : StreamDataReader<string>
    {
        public StreamLinesDataReader(TextReader reader) : base(reader)
        {
        }

        protected override string? ReadOverride(string? line)
        {
            return line;
        }
    }
}
