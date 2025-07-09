using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PinkSystem.Net.Http
{
    public interface IReadOnlyHttpHeaders : IEnumerable<KeyValuePair<string, IEnumerable<string>>>
    {
        IEnumerable<string> GetValues(string key);
        bool TryGetValues(string key, [NotNullWhen(true)] out IEnumerable<string>? values);
        void CopyTo(HttpHeaders headers);
    }

    public sealed class HttpHeaders : IReadOnlyHttpHeaders
    {
        private readonly Dictionary<string, List<string>> _dictionary;

        public HttpHeaders(int capacity)
        {
            _dictionary = new(capacity, StringComparer.OrdinalIgnoreCase);
        }

        public HttpHeaders()
        {
            _dictionary = new(StringComparer.OrdinalIgnoreCase);
        }

        public static IReadOnlyHttpHeaders Empty { get; } = new HttpHeaders();

        public void Add(string key, string value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);

            if (_dictionary.TryGetValue(key, out var list))
            {
                list.Add(value);
            }
            else
            {
                _dictionary.Add(key, list = new(1));

                list.Add(value);
            }
        }

        public void Add(string key, IEnumerable<string> values)
        {
            ArgumentNullException.ThrowIfNull(key);

            if (values == null || values.Any(x => x == null))
                throw new ArgumentNullException(nameof(values));

            if (_dictionary.TryGetValue(key, out var list))
                list.AddRange(values);
            else
                _dictionary.Add(key, new(values));
        }

        public void Replace(string key, string value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);

            if (_dictionary.TryGetValue(key, out var list))
            {
                list.Clear();

                list.Add(value);
            }
            else
            {
                _dictionary.Add(key, list = new(1)
                {
                    value
                });
            }
        }

        public void Replace(string key, IEnumerable<string> values)
        {
            if (_dictionary.TryGetValue(key, out var list))
            {
                list.Clear();

                list.AddRange(values);
            }
            else
            {
                _dictionary.Add(key, new(values));
            }
        }

        public IEnumerable<string> GetValues(string key)
        {
            if (!TryGetValues(key, out var values))
                throw new KeyNotFoundException($"Header with name '{key}' not found");

            return values;
        }

        public bool TryGetValues(string key, [NotNullWhen(true)] out IEnumerable<string>? values)
        {
            var result = _dictionary.TryGetValue(key, out var list);

            values = list;

            return result;
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
