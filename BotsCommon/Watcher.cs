using System;
using System.Collections.Generic;

namespace BotsCommon
{
    public sealed class WatcherCollection<T>
    {
        private readonly List<T> _items = new();
        private readonly object _lock = new();

        public void Add(T item)
        {
            lock (_lock)
                _items.Add(item);
        }

        public void Remove(T item)
        {
            lock (_lock)
                _items.Remove(item);
        }

        public IEnumerable<T> GetAll()
        {
            lock (_lock)
                return _items.ToArray();
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
