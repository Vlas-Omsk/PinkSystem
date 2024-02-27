#nullable enable

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Buffers;
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

        private sealed class JsonReaderStream : Stream
        {
            private readonly JsonReader _reader;
            private readonly MemoryStream _memory;
            private readonly JsonWriter _writer;
            private long _position;

            public JsonReaderStream(JsonReader reader)
            {
                _reader = reader;
                _memory = new MemoryStream(0);
                _writer = new JsonTextWriter(new StreamWriter(_memory));
            }

            public override bool CanRead { get; } = true;
            public override bool CanSeek { get; } = false;
            public override bool CanWrite { get; } = false;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => _position;
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                while (_memory.Position < count - 1)
                {
                    if (!_reader.Read())
                        break;

                    _writer.WriteToken(_reader, false);
                    _writer.Flush();
                }

                var memoryLength = (int)_memory.Position;

                count = Math.Min(memoryLength, count);

                _memory.Position = 0;

                var readedLength = _memory.Read(buffer, offset, count);

                if (readedLength != count)
                    throw new Exception("Readed length not equals to count");

                var unreadLength = memoryLength - count;

                if (unreadLength > 0)
                {
                    var unreadBuffer = ArrayPool<byte>.Shared.Rent(unreadLength);

                    _memory.Read(unreadBuffer);

                    _memory.Position = 0;

                    _memory.Write(unreadBuffer);

                    ArrayPool<byte>.Shared.Return(unreadBuffer);
                }
                else
                {
                    _memory.Position = 0;
                }

                _position += count;

                return count;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            protected override void Dispose(bool disposing)
            {
                ((IDisposable)_reader).Dispose();
                ((IDisposable)_writer).Dispose();
                _memory.Dispose();

                base.Dispose(disposing);
            }
        }

        public static Stream ToStream(this JsonReader self)
        {
            return new JsonReaderStream(self);
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

        public static IEnumerable<T> ValuesRequired<T>(this JToken self)
        {
            return self.Values<T?>().Select(
                x => x ?? throw new Exception($"Value cannot be converted to type {typeof(T)}")
            ) ?? throw new Exception($"Value cannot be converted to type {typeof(T)}");
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

        public static JProperty AsProperty(this JToken self)
        {
            return (JProperty)self;
        }
    }
}
