namespace BotsCommon.IO.Data
{
    public interface IDataReader<out T> : IDisposable
    {
        int? Length { get; }
        int Index { get; }

        T Read();
        void Reset();
    }
}
