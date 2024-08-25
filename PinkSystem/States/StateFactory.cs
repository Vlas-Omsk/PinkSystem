using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PinkSystem.States
{
    public sealed class StateFactory : IStateFactory
    {
        private readonly ConcurrentDictionary<string, State> _states = new();
        private readonly ImmutableArray<IStateProvider> _providers;

        public StateFactory(IEnumerable<IStateProvider> providers)
        {
            _providers = providers.ToImmutableArray();
        }

        public IState Create(string category)
        {
            var newState = new State(this);
            var state = _states.GetOrAdd(category, newState);

            if (newState != state)
                throw new Exception($"State for category '{category}' alredy created");

            return state;
        }

        public IState GetOrCreate(string category)
        {
            var state = _states.GetOrAdd(category, (_) => new State(this));

            return state;
        }

        internal void NotifyUpdate()
        {
            var container = new StateContainer(
                _states
                    .ToArray()
                    .Where(x => x.Value.Value != null)
                    .Select(x => new KeyValuePair<string, IEnumerable<KeyValuePair<string, string>>>(x.Key, x.Value.Value!))
            );

            foreach (var provider in _providers)
                provider.Set(container);
        }

        public void Dispose()
        {
            foreach (var provider in _providers)
                provider.Dispose();
        }
    }
}
