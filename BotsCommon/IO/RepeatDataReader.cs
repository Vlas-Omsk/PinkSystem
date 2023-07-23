namespace BotsCommon.IO
{
    public sealed class RepeatDataReader<T> : IDataReader<T>
    {
        private readonly IDataReader<T> _dataReader;

        public RepeatDataReader(IDataReader<T> dataReader)
        {
            _dataReader = dataReader;
        }

        public int Length => _dataReader.Length;
        public int Index => _dataReader.Index;

        public T Read()
        {
            var data = _dataReader.Read();

            if (data == null)
            {
                _dataReader.Reset();

                data = _dataReader.Read();
            }

            return data;
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
