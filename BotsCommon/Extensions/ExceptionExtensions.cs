using System;
using System.Linq;

namespace BotsCommon
{
    public static class ExceptionExtensions
    {
        public static bool CheckAny(this Exception self, Func<Exception, bool> func)
        {
            if (self is AggregateException aggregateException)
                return aggregateException.InnerExceptions.Any(func);

            return func(self);
        }

        public static bool CheckAll(this Exception self, Func<Exception, bool> func)
        {
            if (self is AggregateException aggregateException)
                return aggregateException.InnerExceptions.All(func);

            return func(self);
        }
    }
}
