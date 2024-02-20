#nullable enable

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

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

        public static JToken SelectTokenRequired(this JToken self, string path)
        {
            return self.SelectToken(path, new JsonSelectSettings()
            {
                ErrorWhenNoMatch = true,
            }) ?? throw new Exception($"Cannot find '{path}' in json");
        }

        public static T ValueRequired<T>(this JToken self)
        {
            return self.Value<T?>() ?? throw new Exception($"Value cannot be converted to type {typeof(T)}");
        }

        public static JArray AsArray(this JToken self)
        {
            return (JArray)self;
        }

        public static JObject AsObject(this JToken self)
        {
            return (JObject)self;
        }

        public static JValue AsValue(this JToken self)
        {
            return (JValue)self;
        }
    }
}
