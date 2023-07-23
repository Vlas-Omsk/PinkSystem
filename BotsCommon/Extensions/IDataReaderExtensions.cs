using BotsCommon.IO;

namespace BotsCommon
{
    public static class IDataReaderExtensions
    {
        public static float GetProgress<T>(this IDataReader<T> self)
        {
            return (float)self.Index / self.Length * 100;
        }
    }
}
