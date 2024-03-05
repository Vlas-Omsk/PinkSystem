using System;

namespace BotsCommon.IO.Data
{
    public interface IUsageLimiter<in T> : IDisposable
    {
        int GetNumberOfUses(T item);
        void IncreaseNumberOfUses(T item);
    }
}
