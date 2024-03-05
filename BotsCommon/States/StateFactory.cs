using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace BotsCommon.States
{
    public sealed class StateFactory : IStateFactory
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, State> _states = new();
        private readonly ImmutableArray<IStateProvider> _providers;

        public StateFactory(IEnumerable<IStateProvider> providers)
        {
            _providers = providers.ToImmutableArray();
        }

        public IState Create(string category)
        {
            State? state;

            lock (_lock)
            {
                if (_states.Any(x => x.Key == category))
                    throw new Exception($"State for category '{category}' alredy created");
                
                state = new State(this);

                _states.Add(category, state);
            }

            return state;
        }

        public IState GetOrCreate(string category)
        {
            lock (_lock)
            {
                if (!_states.TryGetValue(category, out var state))
                    _states.Add(category, state = new State(this));

                return state;
            }
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
