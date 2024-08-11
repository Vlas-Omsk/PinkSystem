using System.IO;

namespace BotsCommon.IO.Data
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
