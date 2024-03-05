using Newtonsoft.Json;
using System;
using System.IO;

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
            var memoryStream = new MemoryStream(256);

            using (var writer = new JsonTextWriter(new StreamWriter(memoryStream, leaveOpen: true)))
            {
                var serializer = new JsonSerializer();

                serializer.Serialize(writer, obj);
            }

            return memoryStream.ToReadOnlyMemory();
        }
    }
}
