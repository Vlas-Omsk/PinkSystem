using System.Collections.Generic;

namespace BotsCommon.States
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

        public static NullState Instance { get; } = new();
    }
}
