using PinkSystem.IO.Data;
using System.Collections.Generic;

namespace PinkSystem
{
    public static class IDataReaderExtensions
    {
        public static RepeatDataReader<T> AsRepeatable<T>(this IDataReader<T> self)
        {
            return new RepeatDataReader<T>(self);
        }

        public static RepeatDataReader AsRepeatable(this IDataReader self)
        {
            return new RepeatDataReader(self);
        }

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
