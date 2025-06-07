using System;

namespace PinkSystem.IO.Data
{
    public interface IDataWriter<in T> : IDisposable
    {
        void Write(T data);
        void Flush();
    }
}
