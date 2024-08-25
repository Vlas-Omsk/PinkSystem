using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using PinkSystem.IO.Data;

namespace PinkSystem.RegularExpressions
{
    public sealed class RegexFilter
    {
        private readonly ImmutableArray<Regex> _filters;

        public RegexFilter(IEnumerable<Regex> filters)
        {
            _filters = filters.ToImmutableArray();
        }

        public bool IsMatch(string input)
        {
            return _filters.Any(x => x.IsMatch(input));
        }

        public static RegexFilter FromReader(IDataReader<Regex> reader)
        {
            var regexes = new List<Regex>();
            Regex? filter;

            while ((filter = reader.Read()) != null)
                regexes.Add(filter);

            return new RegexFilter(regexes);
        }
    }
}
