using BotsCommon.Runtime;
using System;
using System.IO;

namespace BotsCommon
{
    public static class MemoryStreamExtensions
    {
        public static ReadOnlyMemory<byte> ToReadOnlyMemory(this MemoryStream self)
        {
            var accessor = new ObjectAccessor(self, typeof(MemoryStream));

            var buffer = (byte[])accessor.GetField("_buffer")!;
            var origin = (int)accessor.GetField("_origin")!;

            return new ReadOnlyMemory<byte>(
                buffer,
                origin,
                (int)self.Length
            );
        }
    }
}
