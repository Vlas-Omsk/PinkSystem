using BotsCommon.IO.Content;
using Newtonsoft.Json;
using System.Buffers;

namespace BotsCommon
{
    public static class IContentReaderExtensions
    {
        public static JsonTextReader ReadAsJsonStream(this IContentReader self)
        {
            return new JsonTextReader(new StreamReader(self.CreateStream()));
        }

        public static string ReadAsString(this IContentReader self)
        {
            using var streamReader = new StreamReader(self.CreateStream());

            return streamReader.ReadToEnd();
        }

        public static ReadOnlyMemory<byte> ReadAsBytes(this IContentReader self)
        {
            using var stream = self.CreateStream();
            using var memoryStream = new MemoryStream();

            stream.CopyTo(memoryStream);

            return memoryStream.ToReadOnlyMemory();
        }
    }
}
