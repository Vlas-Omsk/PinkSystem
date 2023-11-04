namespace BotsCommon.IO.Data
{
    public sealed class RandomDataReader<T> : IDataReader<T>
    {
        private readonly IDataReader<T> _dataReader;

        public RandomDataReader(IDataReader<T> dataReader)
        {
            _dataReader = dataReader;
        }

        public int Length => _dataReader.Length;
        public int Index => _dataReader.Index;

        public T Read()
        {
            var skipCount = Random.Shared.Next(0, _dataReader.Length - _dataReader.Index);

            for (var i = 0; i < skipCount; i++)
                _dataReader.Read();

            return _dataReader.Read();
        }

        public void Reset()
        {
            _dataReader.Reset();
        }

        public void Dispose()
        {
            _dataReader.Dispose();
        }
    }
}
