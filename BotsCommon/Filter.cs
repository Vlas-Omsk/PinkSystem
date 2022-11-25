using System;
using System.Text.RegularExpressions;
using BotsCommon.IO;

namespace BotsCommon
{
    public sealed class Filter
    {
        private readonly Regex[] _regexes;

        public Filter(IDataReader<string> reader)
        {
            var regexes = new List<Regex>();
            string line;

            while ((line = reader.Read()) != null)
                regexes.Add(new Regex(line, RegexOptions.Compiled));

            _regexes = regexes.ToArray();
        }

        public bool IsMatch(string input)
        {
            return _regexes.Any(x => x.IsMatch(input));
        }
    }
}
