using System;
using System.IO;
using Newtonsoft.Json;
using PinkSystem.IO.Content;

namespace PinkSystem
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
            using var memoryStream = self.Length.HasValue && self.Length.Value <= int.MaxValue ?
                new MemoryStream((int)self.Length.Value) :
                new MemoryStream();

            stream.CopyTo(memoryStream);

            return memoryStream.ToReadOnlyMemory();
        }
    }
}
