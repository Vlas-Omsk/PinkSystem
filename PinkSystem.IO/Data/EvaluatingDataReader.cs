using PinkSystem.Text.Evaluating;

namespace PinkSystem.IO.Data
{
    public sealed class EvaluatingDataReader : IDataReader<string>
    {
        private readonly IDataReader<string> _reader;
        private readonly StringEvaluator _stringEvaluator;

        public EvaluatingDataReader(IDataReader<string> reader, StringEvaluator stringEvaluator)
        {
            _reader = reader;
            _stringEvaluator = stringEvaluator;
        }

        public int? Length => _reader.Length;
        public int Index => _reader.Index;

        public string? Read()
        {
            var data = _reader.Read();

            if (data == null)
                return null;

            return _stringEvaluator.Evaluate(data);
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
