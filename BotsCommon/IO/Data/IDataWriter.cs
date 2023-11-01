namespace BotsCommon.IO.Data
{
    public interface IDataWriter<in T> : IDisposable
    {
        void Write(T data);
        void Flush();
    }
}
