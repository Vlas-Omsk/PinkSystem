using BotsCommon.IO.Data;

namespace BotsCommon
{
    public static class IDataReaderExtensions
    {
        public static float GetProgress<T>(this IDataReader<T> self)
        {
            return self.Length.HasValue ?
                (float)self.Index / self.Length.Value * 100 :
                float.PositiveInfinity;
        }
    }
}
