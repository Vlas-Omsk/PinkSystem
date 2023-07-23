namespace BotsCommon.IO
{
    public interface IDataWriter<in T> : IDisposable
    {
        void Write(T data);
        void Flush();
    }
}
