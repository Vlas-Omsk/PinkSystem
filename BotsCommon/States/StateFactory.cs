#nullable enable

namespace BotsCommon.States
{
    public sealed class StateFactory : IStateFactory
    {
        private readonly object _lock = new();
        private readonly List<KeyValuePair<string?, State>> _states = new();
        private readonly List<IStateProvider> _providers = new();

        public IState Create(string? category)
        {
            State? state;

            lock (_lock)
            {
                if (_states.Any(x => x.Key == category))
                    throw new Exception($"State for category '{category}' alredy created");
                
                state = new State(this);

                _states.Add(new KeyValuePair<string?, State>(category, state));
            }

            return state;
        }

        public IState GetOrCreate(string? category)
        {
            lock (_lock)
            {
                var keyValue = _states.Cast<KeyValuePair<string?, State>?>().FirstOrDefault(x => x!.Value.Key == category);

                if (keyValue == null)
                {
                    keyValue = new KeyValuePair<string?, State>(category, new State(this));

                    _states.Add(keyValue.Value);
                }

                return keyValue.Value.Value;
            }
        }

        public void AddProvider(IStateProvider provider)
        {
            lock (_lock)
                _providers.Add(provider);
        }

        internal void NotifyUpdate()
        {
            var container = new StateContainer();

            lock (_lock)
                foreach (var progress in _states)
                    if (progress.Value.Value != null)
                        container.Add(progress.Key, progress.Value.Value);

            lock (_lock)
                foreach (var provider in _providers)
                    provider.Set(container);
        }

        public void Dispose()
        {
            lock (_lock)
                foreach (var provider in _providers)
                    provider.Dispose();
        }
    }
}
