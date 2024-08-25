using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PinkSystem
{
    public sealed class WatcherCollection<T>
        where T : notnull
    {
        private readonly ConcurrentDictionary<T, bool> _items = new();

        public void Add(T item)
        {
            _items.TryAdd(item, true);
        }

        public void Remove(T item)
        {
            _items.TryRemove(item, out _);
        }

        public IEnumerable<T> GetAll()
        {
            return _items.Select(x => x.Key);
        }
    }

    public sealed class Watcher<T>
        where T : notnull
    {
        public WatcherCollection<T> Collection { get; } = new();

        public void Invoke(Action<T> action)
        {
            foreach (var item in Collection.GetAll())
                action(item);
        }
    }
}
