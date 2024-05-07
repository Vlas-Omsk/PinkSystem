using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BotsCommon.Net
{
    public sealed class QueryData : IEnumerable<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, IEnumerable<string>>>
    {
        private readonly Dictionary<string, List<string>> _dictionary;

        public QueryData(IEnumerable<KeyValuePair<string, IEnumerable<string>>> dictionary)
        {
            _dictionary = dictionary.ToDictionary(x => x.Key, x => x.Value.ToList());
        }

        public QueryData()
        {
            _dictionary = new();
        }

        public static QueryData Empty { get; } = new();

        public string this[string key]
        {
            get => _dictionary[key].First(),
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

        IEnumerator<KeyValuePair<string, IEnumerable<string>>> IEnumerable<KeyValuePair<string, IEnumerable<string>>>.GetEnumerator()
        {
            return _dictionary
                .Select(x => new KeyValuePair<string, IEnumerable<string>>(x.Key, x.Value))
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

        public static QueryData Parse(string query)
        {
            var queryData = new QueryData();
            var nameValueCollection = HttpUtility.ParseQueryString(query);

            foreach (string key in nameValueCollection)
            {
                var values = nameValueCollection.GetValues(key)!;

                queryData.Add(key, values);
            }

            return queryData;
        }

        public static QueryData ParseFromUri(string uri)
        {
            return Parse(new Uri(uri).Query);
        }

        public static QueryData FromFlat(IEnumerable<KeyValuePair<string, string>> flat)
        {
            return new QueryData(flat.Select(x => new KeyValuePair<string, IEnumerable<string>>(x.Key, [x.Value])));
        }
    }
}
