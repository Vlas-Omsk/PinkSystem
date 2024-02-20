using Newtonsoft.Json;

namespace BotsCommon.IO.Content
{
    public sealed class JsonContentReader : ByteArrayContentReader
    {
        public JsonContentReader(object obj) :
            base(GetBytesFromData(obj), "application/json; charset=UTF-8")
        {
            
        }

        private static ReadOnlyMemory<byte> GetBytesFromData(object obj)
        {
            var memoryStream = new MemoryStream(0);
            var streamWriter = new StreamWriter(memoryStream);

            using var writer = new JsonTextWriter(streamWriter);

            var serializer = new JsonSerializer();

            serializer.Serialize(writer, obj);

            return memoryStream.ToReadOnlyMemory();
        }
    }
}
