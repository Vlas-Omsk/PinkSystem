using PinkJson2;
using System.Text;

namespace BotsCommon.IO.Content
{
    public sealed class JsonContentReader : ByteArrayContentReader
    {
        public JsonContentReader(IEnumerable<JsonEnumerableItem> data) : this(data, TypeConverter.Default)
        {
        }

        public JsonContentReader(IEnumerable<JsonEnumerableItem> data, TypeConverter typeConverter) :
            base(GetBytesFromData(data, typeConverter), "application/json; charset=UTF-8")
        {
            
        }

        private static ReadOnlyMemory<byte> GetBytesFromData(IEnumerable<JsonEnumerableItem> data, TypeConverter typeConverter)
        {
            using (var memoryStream = new MemoryStream(0))
            {
                using (var streamWriter = new StreamWriter(memoryStream, new UTF8Encoding(false), 1024, true))
                    data.ToStream(streamWriter, typeConverter);

                return memoryStream.AsReadOnlyMemory();
            }
        }
    }
}
