using Newtonsoft.Json;
using System;
using System.IO;

namespace BotsCommon.IO.Content
{
    public sealed class JsonContentReader : ByteArrayContentReader
    {
        public JsonContentReader(object obj, JsonSerializer? serializer = null) :
            base(GetBytesFromData(obj, serializer), "application/json; charset=UTF-8")
        {
            
        }

        private static ReadOnlyMemory<byte> GetBytesFromData(object obj, JsonSerializer? serializer)
        {
            serializer ??= new JsonSerializer();

            var memoryStream = new MemoryStream(256);

            using (var writer = new JsonTextWriter(new StreamWriter(memoryStream, leaveOpen: true)))
            {
                serializer.Serialize(writer, obj);
            }

            return memoryStream.ToReadOnlyMemory();
        }
    }
}
