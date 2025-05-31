using System;

namespace PinkSystem.IO.Data
{
    public class ConvertDataReader<TIn, TOut> : IDataReader<TOut>
    {
        private readonly IDataReader<TIn> _reader;
        private readonly Func<TIn, TOut> _converter;

        public ConvertDataReader(IDataReader<TIn> reader, Func<TIn, TOut> converter)
        {
            _reader = reader;
            _converter = converter;
        }

        public int? Length => _reader.Length;
        public int Index => _reader.Index;

        public TOut? Read()
        {
            var item = _reader.Read();

            if (item == null)
                return default;

            return _converter(item);
        }

        public void Reset()
        {
            _reader.Reset();
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
