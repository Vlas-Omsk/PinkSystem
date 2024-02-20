using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text;

namespace BotsCommon
{
    public static class JsonExtensions
    {
        public static JToken SerializeToJToken(this object self, JsonSerializer serializer)
        {
            return JToken.FromObject(self, serializer);
        }

        public static string SerializeToJString(this object self, JsonSerializer serializer)
        {
            var stringBuilder = new StringBuilder(256);
            var stringWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);

            using (var writer = new JsonTextWriter(stringWriter))
                serializer.Serialize(writer, self);

            return stringWriter.ToString();
        }
    }
}
