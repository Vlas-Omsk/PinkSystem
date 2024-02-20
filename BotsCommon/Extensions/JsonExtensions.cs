#nullable enable

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

        public static JToken SelectPath(this JToken self, params object[] path)
        {
            foreach (var name in path)
                self = self[name] ?? throw new Exception($"Cannot find '{name}' in json by path '{string.Join('.', path)}'");

            return self;
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
