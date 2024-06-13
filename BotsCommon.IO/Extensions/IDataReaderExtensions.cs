using BotsCommon.IO.Data;
using System.Collections.Generic;

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

        public static IEnumerable<T> AsEnumerable<T>(this IDataReader<T> self)
        {
            T? item;

            while ((item = self.Read()) != null)
            {
                yield return item;
            }
        }
    }
}
