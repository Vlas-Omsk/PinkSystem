using System;
using System.Linq;

namespace PinkSystem
{
    public static class ExceptionExtensions
    {
        public static bool CheckAny(this Exception self, Func<Exception, bool> func)
        {
            if (self is AggregateException aggregateException)
                return aggregateException.InnerExceptions.Any(x => CheckAny(x, func));

            return func(self);
        }

        public static bool CheckAll(this Exception self, Func<Exception, bool> func)
        {
            if (self is AggregateException aggregateException)
                return aggregateException.InnerExceptions.All(x => CheckAll(x, func));

            return func(self);
        }
    }
}
