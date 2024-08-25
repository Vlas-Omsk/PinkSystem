using System;

namespace PinkSystem.IO.Data
{
    public interface IDataReader : IDisposable
    {
        int? Length { get; }
        int Index { get; }

        object? Read();
        void Reset();
    }

    public interface IDataReader<out T> : IDataReader
    {
        new T? Read();
    }
}
