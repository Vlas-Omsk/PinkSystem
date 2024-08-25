using System.Collections.Generic;

namespace PinkSystem.States
{
    public sealed class StateContainer
    {
        private readonly Dictionary<string, IEnumerable<KeyValuePair<string, string>>> _values;

        public StateContainer()
        {
            _values = new();
        }

        public StateContainer(IEnumerable<KeyValuePair<string, IEnumerable<KeyValuePair<string, string>>>> values)
        {
            _values = new(values);
        }

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
