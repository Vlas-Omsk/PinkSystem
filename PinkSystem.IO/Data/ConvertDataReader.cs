using System;

namespace PinkSystem.IO.Data
{
    public class ConvertDataReader : IDataReader
    {
        private readonly IDataReader _reader;
        private readonly Func<object, object> _converter;

        public ConvertDataReader(IDataReader reader, Func<object, object> converter)
        {
            _reader = reader;
            _converter = converter;
        }

        public int? Length => _reader.Length;
        public int Index => _reader.Index;

        public object? Read()
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

        object? IDataReader.Read()
        {
            return Read();
        }
    }

    public sealed class ConvertDataReader<TIn, TOut> : ConvertDataReader, IDataReader<TOut>
    {
        public ConvertDataReader(IDataReader<TIn> reader, Func<TIn, TOut> converter) : base(reader, (obj) => converter((TIn)obj)!)
        {
        }

        TOut? IDataReader<TOut>.Read()
        {
            return (TOut?)Read();
        }
    }
}
