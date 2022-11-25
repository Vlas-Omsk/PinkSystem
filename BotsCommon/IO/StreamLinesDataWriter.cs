using System;

namespace BotsCommon.IO
{
    public sealed class StreamLinesDataWriter : StreamDataWriter<string>
    {
        public StreamLinesDataWriter(StreamWriter writer) : base(writer)
        {
        }

        protected override string WriteOverride(string data)
        {
            return data;
        }
    }
}
