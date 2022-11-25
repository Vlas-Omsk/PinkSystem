using System;

namespace BotsCommon.IO
{
    public interface IUsageLimiter<in T>
    {
        int GetNumberOfUses(T item);
        void IncreaseNumberOfUses(T item);
    }
}
