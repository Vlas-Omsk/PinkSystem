using System;
using System.Collections.Generic;

namespace PinkSystem
{
    public static class ExceptionExtensions
    {
        public static IEnumerable<Exception> Enumerate(this Exception self)
        {
            if (self is AggregateException aggregateException)
            {
                foreach (var ex in aggregateException.InnerExceptions)
                {
                    yield return ex;
                }
            }

            yield return self;
        }
    }
}
