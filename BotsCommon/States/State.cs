using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BotsCommon.States
{
    public sealed class State : IState
    {
        private readonly StateFactory _factory;
        private ConcurrentDictionary<string, string>? _value = new();
        private readonly object _lock = new();

        public State(StateFactory factory)
        {
            _factory = factory;
        }

        public IEnumerable<KeyValuePair<string, string>>? Value => _value;

        public void Set(IEnumerable<KeyValuePair<string, string>> value)
        {
            lock (_lock)
            {
                if (_value == null)
                {
                    _value = new ConcurrentDictionary<string, string>(value);
                }
                else
                {
                    _value.Clear();

                    foreach (var keyValue in value)
                        _value.TryAdd(keyValue.Key, keyValue.Value);
                }
            }

            _factory.NotifyUpdate();
        }

        public void Change(string key, string value)
        {
            lock (_lock)
            {
                if (_value == null)
                    _value = new ConcurrentDictionary<string, string>();
            }

            _value.AddOrUpdate(key, (_) => value, (_, _) => value);

            _factory.NotifyUpdate();
        }

        public void Remove(string key)
        {
            lock (_lock)
            {
                if (_value == null)
                    return;
            }

            _value.TryRemove(key, out _);

            _factory.NotifyUpdate();
        }
    }
}
