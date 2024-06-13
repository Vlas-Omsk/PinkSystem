using System.Collections.Generic;

namespace BotsCommon.IO.Data
{
    public sealed class ListDataReader<T> : IDataReader<T>
    {
        private readonly IReadOnlyList<T> _list;
        private readonly object _lock = new();
        private int _index;

        public ListDataReader(IReadOnlyList<T> list)
        {
            _list = list;
        }

        public int? Length { get; }
        public int Index
        {
            get
            {
                lock (_lock)
                    return _index;
            }
            set
            {
                lock (_lock)
                    _index = value;
            }
        }

        public T? Read()
        {
            int index;

            lock (_lock)
            {
                if (Index >= _list.Count)
                    return default;

                index = Index++;
            }

            return _list[index];
        }

        public void Reset()
        {
            lock (_lock)
            {
                Index = 0;
            }
        }

        public void Dispose()
        {
        }
    }
}
