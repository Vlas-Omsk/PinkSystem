namespace BotsCommon
{
    public static class MemoryStreamExtensions
    {
        public static ReadOnlyMemory<byte> AsReadOnlyMemory(this MemoryStream self)
        {
            return new ReadOnlyMemory<byte>(
                self.GetBuffer() ?? Array.Empty<byte>(),
                0,
                (int)self.Length
            );
        }
    }
}
