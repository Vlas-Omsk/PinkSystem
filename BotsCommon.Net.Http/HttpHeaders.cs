using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BotsCommon.Net.Http
{
    public sealed class HttpHeaders : IEnumerable<KeyValuePair<string, IEnumerable<string>>>
    {
        private readonly Dictionary<string, List<string>> _dictionary = new(StringComparer.InvariantCultureIgnoreCase);

        public void Add(string key, string value)
        {
            var list = GetList(key);

            list.Add(value);
        }

        public void Add(string key, IEnumerable<string> values)
        {
            var list = GetList(key);

            list.AddRange(values);
        }

        public void Replace(string key, string value)
        {
            var list = GetList(key);

            list.Clear();

            list.Add(value);
        }

        public void Replace(string key, IEnumerable<string> values)
        {
            var list = GetList(key);

            list.Clear();

            list.AddRange(values);
        }

        public bool TryGetValues(string key, [NotNullWhen(true)] out IEnumerable<string>? values)
        {
            var result = _dictionary.TryGetValue(key, out var list);

            values = list;

            return result;
        }

        private List<string> GetList(string key)
        {
            if (!_dictionary.TryGetValue(key, out var list))
                _dictionary.Add(key, list = new());

            return list;
        }

        public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator()
        {
            return _dictionary.Select(
                x => new KeyValuePair<string, IEnumerable<string>>(x.Key, x.Value)
            ).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(HttpHeaders headers)
        {
            foreach (var header in _dictionary)
                headers.Add(header.Key, header.Value);
        }
    }
}
