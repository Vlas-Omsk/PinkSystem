using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PinkSystem.Net
{
    public interface IReadOnlyQueryData : IEnumerable<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, IEnumerable<string>>>
    {
        string this[string key] { get; }

        string ToString();
    }

    public sealed class QueryData : IReadOnlyQueryData
    {
        private readonly Dictionary<string, List<string>> _dictionary;
        private string? _stringPresentation;

        public QueryData(IEnumerable<KeyValuePair<string, IEnumerable<string>>> dictionary)
        {
            _dictionary = dictionary.ToDictionary(x => x.Key, x => x.Value.ToList());
        }

        public QueryData()
        {
            _dictionary = new();
        }

        public static IReadOnlyQueryData Empty { get; } = new QueryData();

        public string this[string key]
        {
            get => _dictionary[key].First();
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

            _stringPresentation = null;

            return this;
        }

        public QueryData Add(string key, IEnumerable<string> values)
        {
            if (_dictionary.TryGetValue(key, out var list))
                list.AddRange(values);
            else
                _dictionary.Add(key, list = new List<string>(values));

            _stringPresentation = null;

            return this;
        }

        public override string ToString()
        {
            if (_stringPresentation != null)
                return _stringPresentation;

            return _stringPresentation = string.Join(
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

                if (key == null && values.Length == 1)
                    queryData.Add(values[0], "");
                else if (key == null)
                    throw new Exception("Key in query cannot be null");
                else
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
