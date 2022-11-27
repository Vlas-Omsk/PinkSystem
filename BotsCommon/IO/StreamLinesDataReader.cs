using System;

namespace BotsCommon.IO
{
    public sealed class StreamLinesDataReader : StreamDataReader<string>
    {
        public StreamLinesDataReader(StreamReader reader) : base(reader)
        {
        }

        public StreamLinesDataReader(string path) : base(new StreamReader(path))
        {
        }

        protected override string ReadOverride(string line)
        {
            return line;
        }
    }
}
