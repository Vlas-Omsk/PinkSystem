using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace PinkSystem.States
{
    public sealed class State : IState
    {
        private readonly StateFactory _factory;
        private ValueContainer? _value;

        private sealed class ValueContainer
        {
            private readonly IEnumerable<KeyValuePair<string, string>> _enumerable;
            private ConcurrentDictionary<string, string>? _dictionary;

            public ValueContainer(IEnumerable<KeyValuePair<string, string>> enumerable)
            {
                _enumerable = enumerable;
            }

            public ConcurrentDictionary<string, string> Value
            {
                get
                {
                    Interlocked.CompareExchange(ref _dictionary, new ConcurrentDictionary<string, string>(_enumerable), null);

                    return _dictionary;
                }
            }
                
        }

        public State(StateFactory factory)
        {
            _factory = factory;
        }

        public IEnumerable<KeyValuePair<string, string>>? Value => _value?.Value;

        public void Set(IEnumerable<KeyValuePair<string, string>> value)
        {
            if (Interlocked.CompareExchange(ref _value, new ValueContainer(value), null) != null)
            {
                _value.Value.Clear();

                foreach (var keyValue in value)
                    _value.Value.TryAdd(keyValue.Key, keyValue.Value);
            }
            else
            {
                // Copying enumerable if exchanged
                _ = _value.Value;
            }

            _factory.NotifyUpdate();
        }

        public void Change(string key, string value)
        {
            Interlocked.CompareExchange(ref _value, new ValueContainer([]), null);

            _value.Value.AddOrUpdate(key, (_) => value, (_, _) => value);

            _factory.NotifyUpdate();
        }

        public void Remove(string key)
        {
            if (_value == null)
                return;

            _value.Value.TryRemove(key, out _);

            _factory.NotifyUpdate();
        }
    }
}
