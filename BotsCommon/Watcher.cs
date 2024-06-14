using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS8714

namespace BotsCommon
{
    public sealed class WatcherCollection<T>
    {
        private readonly ConcurrentDictionary<T, object?> _items = new();

        public void Add(T item)
        {
            _items.TryAdd(item, null);
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
    {
        public WatcherCollection<T> Collection { get; } = new();

        public void Invoke(Action<T> action)
        {
            foreach (var item in Collection.GetAll())
                action(item);
        }
    }
}
