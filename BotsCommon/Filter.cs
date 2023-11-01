using System.Text.RegularExpressions;
using BotsCommon.IO.Data;

namespace BotsCommon
{
    public sealed class Filter
    {
        private readonly Regex[] _regexes;

        public Filter(Regex[] regexes)
        {
            _regexes = regexes;
        }

        public bool IsMatch(string input)
        {
            return _regexes.Any(x => x.IsMatch(input));
        }

        public static Filter FromReader(IDataReader<string> reader)
        {
            var regexes = new List<Regex>();
            string line;

            while ((line = reader.Read()) != null)
                regexes.Add(new Regex(line, RegexOptions.Compiled));

            return new Filter(regexes.ToArray());
        }
    }
}
