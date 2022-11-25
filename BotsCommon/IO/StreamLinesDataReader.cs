using System;

namespace BotsCommon.IO
{
    public sealed class StreamLinesDataReader : StreamDataReader<string>
    {
        public StreamLinesDataReader(StreamReader reader) : base(reader)
        {
        }

        protected override string ReadOverride(string line)
        {
            return line;
        }
    }
}
