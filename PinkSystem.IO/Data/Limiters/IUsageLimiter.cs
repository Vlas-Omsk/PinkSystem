using System;

namespace PinkSystem.IO.Data.Limiters
{
    public interface IUsageLimiter<in T> : IDisposable
    {
        int GetNumberOfUses(T item);
        void IncreaseNumberOfUses(T item);
    }
}
