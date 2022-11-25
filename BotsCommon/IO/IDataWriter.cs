using System;

namespace BotsCommon.IO
{
    public interface IDataWriter<in T>
    {
        void Write(T data);
        void Flush();
    }
}
