namespace BotsCommon.IO
{
    public sealed class StreamLinesDataWriter : StreamDataWriter<string>
    {
        public StreamLinesDataWriter(StreamWriter writer) : base(writer)
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
