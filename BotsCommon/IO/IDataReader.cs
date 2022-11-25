using System;

namespace BotsCommon.IO
{
    public interface IDataReader<out T>
    {
        int Length { get; }
        int Index { get; }

        T Read();
        void Reset();
    }
}
