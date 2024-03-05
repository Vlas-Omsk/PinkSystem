using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BotsCommon.IO.Data;

namespace BotsCommon.RegularExpressions
{
    public sealed class RegexFilter
    {
        private readonly Regex[] _regexes;

        public RegexFilter(Regex[] regexes)
        {
            _regexes = regexes;
        }

        public bool IsMatch(string input)
        {
            return _regexes.Any(x => x.IsMatch(input));
        }

        public static RegexFilter FromReader(IDataReader<string> reader)
        {
            var regexes = new List<Regex>();
            string? line;

            while ((line = reader.Read()) != null)
                regexes.Add(new Regex(line, RegexOptions.Compiled));

            return new RegexFilter(regexes.ToArray());
        }
    }
}
