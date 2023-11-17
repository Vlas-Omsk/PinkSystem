#nullable enable

namespace BotsCommon.States
{
    public sealed class StateContainer
    {
        private readonly List<KeyValuePair<string?, string>> _values = new();

        public void Add(string? category, string value)
        {
            _values.Add(new KeyValuePair<string?, string>(category, value));
        }

        public IEnumerable<KeyValuePair<string?, string>> Get()
        {
            return _values;
        }
    }
}
