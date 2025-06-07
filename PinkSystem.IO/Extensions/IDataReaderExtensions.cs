using PinkSystem.IO.Data;
using PinkSystem.IO.Evaluating;
using System;
using System.Collections.Generic;
using System.IO;

namespace PinkSystem
{
    public static class IDataReaderExtensions
    {
        public static IDataReader<T> Repeat<T>(this IDataReader<T> self)
        {
            return new RepeatDataReader<T>(self);
        }

        public static IDataReader<TOut> Convert<TIn, TOut>(this IDataReader<TIn> self, Func<TIn, TOut> converter)
        {
            return new ConvertDataReader<TIn, TOut>(self, converter);
        }

        public static IDataReader<string> Evaluate(this IDataReader<string> self)
        {
            return EvaluatingDataReader.CreateDefault(self);
        }

        public static IDataReader<string> Evaluate(this IDataReader<string> self, IEnumerable<IEvaluator> evaluators)
        {
            return new EvaluatingDataReader(self, evaluators);
        }

        public static IDataReader<T> Randomize<T>(this IDataReader<T> self)
        {
            return new RandomDataReader<T>(self);
        }

        public static IDataReader<T> Randomize<T>(this IDataReader<T> self, int maxRange)
        {
            return new RandomDataReader<T>(self, maxRange);
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

        public static IDataReader<T> AsDataReader<T>(this IEnumerable<T> self)
        {
            return new EnumerableDataReader<T>(self);
        }

        public static IDataReader<string> AsDataReader(this TextReader self)
        {
            return new StreamLinesDataReader(self);
        }
    }
}
