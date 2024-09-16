using PinkSystem.IO.Evaluating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PinkSystem.IO.Data
{
    public sealed class EvaluatingDataReader : IDataReader<string>
    {
        private static readonly Regex _functionRegex = new("{(.*?)}", RegexOptions.Compiled);
        private readonly IDataReader<string> _reader;
        private readonly IEvaluator[] _evaluators;

        public EvaluatingDataReader(IDataReader<string> reader, IEnumerable<IEvaluator> evaluators)
        {
            _reader = reader;
            _evaluators = evaluators.ToArray();
        }

        public int? Length => _reader.Length;
        public int Index => _reader.Index;

        public string? Read()
        {
            var data = _reader.Read();

            if (data == null)
                return null;

            data = _functionRegex.Replace(data, x =>
            {
                var args = x.Groups[1].Value.Split(',');

                foreach (var evaluator in _evaluators)
                {
                    if (evaluator.TryEvaluate(args, out var result))
                        return result;
                }

                throw new Exception($"Function '{args[0]}' not supported");
            });

            return data;
        }

        object? IDataReader.Read()
        {
            return Read();
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
