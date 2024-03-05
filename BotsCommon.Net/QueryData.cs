using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BotsCommon.Net
{
    public sealed class QueryData : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, List<string>> _dictionary = new();

        public static QueryData Empty { get; } = new();

        public string this[string key]
        {
            set => Add(key, value);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _dictionary
                .SelectMany(x => 
                    x.Value.Select(c => 
                        new KeyValuePair<string, string>(x.Key, c)
                    )
                )
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public QueryData Add(string key, string value)
        {
            if (_dictionary.TryGetValue(key, out var list))
                list.Add(value);
            else
                _dictionary.Add(key, list = new List<string>() { value });

            return this;
        }

        public QueryData Add(string key, IEnumerable<string> values)
        {
            if (_dictionary.TryGetValue(key, out var list))
                list.AddRange(values);
            else
                _dictionary.Add(key, list = new List<string>(values));

            return this;
        }

        public override string ToString()
        {
            return string.Join(
                '&',
                _dictionary
                    .SelectMany(x =>
                        x.Value.Select(c => (key: x.Key, value: c))
                    )
                    .Select(x =>
                    {
                        var result = Uri.EscapeDataString(x.key);

                        if (string.IsNullOrEmpty(x.value))
                            return result;

                        return result + '=' + Uri.EscapeDataString(x.value);
                    })
            );
        }
    }
}
