using System.Collections.Generic;

namespace BotsCommon.States
{
    public sealed class StateContainer
    {
        private readonly Dictionary<string, IEnumerable<KeyValuePair<string, string>>> _values = new();

        public void Add(string category, IEnumerable<KeyValuePair<string, string>> value)
        {
            _values.Add(category, value);
        }

        public IEnumerable<KeyValuePair<string, IEnumerable<KeyValuePair<string, string>>>> Get()
        {
            return _values;
        }
    }
}
