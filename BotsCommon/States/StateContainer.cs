#nullable enable

namespace BotsCommon.States
{
    public sealed class StateContainer
    {
        private readonly Dictionary<string, string> _values = new();

        public void Add(string category, string value)
        {
            _values.Add(category, value);
        }

        public IEnumerable<KeyValuePair<string, string>> Get()
        {
            return _values;
        }
    }
}
