using BotsCommon.IO.Content;
using PinkJson2;

namespace BotsCommon
{
    public static class IContentReaderExtensions
    {
        public static IEnumerable<JsonEnumerableItem> ReadAsJson(this IContentReader self)
        {
            using var stream = self.CreateStream();

            return Json.Parse(stream).ToArray();
        }

        public static string ReadAsString(this IContentReader self)
        {
            using var stream = self.CreateStream();
            using var streamReader = new StreamReader(stream);

            return streamReader.ReadToEnd();
        }
    }
}
