using System.Collections.Generic;

namespace PinkSystem.States
{
    public sealed class NullState : IState
    {
        private NullState()
        {
        }

        public string? Value { get; } = null;

        public void Set(IEnumerable<KeyValuePair<string, string>> value)
        {
        }

        public void Change(string key, string value)
        {
        }

        public void Remove(string key)
        {
        }

        public static NullState Instance { get; } = new();
    }
}
