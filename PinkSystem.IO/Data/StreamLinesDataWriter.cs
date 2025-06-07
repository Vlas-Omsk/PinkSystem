using System.IO;

namespace PinkSystem.IO.Data
{
    public sealed class StreamLinesDataWriter : StreamDataWriter<string>
    {
        public StreamLinesDataWriter(TextWriter writer) : base(writer)
        {
        }

        public StreamLinesDataWriter(string path) : base(new StreamWriter(path))
        {
        }

        protected override string WriteOverride(string data)
        {
            return data;
        }
    }
}
