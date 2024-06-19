using System.Collections.Generic;
using System.Threading;

namespace BotsCommon.IO.Data
{
    public sealed class ListDataReader<T> : IDataReader<T>
    {
        private readonly IReadOnlyList<T> _list;
        private int _index;

        public ListDataReader(IReadOnlyList<T> list)
        {
            _list = list;
        }

        public int? Length { get; }
        public int Index => _index;

        public T? Read()
        {
            int index = Interlocked.Increment(ref _index) - 1;

            if (index >= _list.Count)
                return default;

            return _list[index];
        }

        public void Reset()
        {
            Interlocked.Exchange(ref _index, 0);
        }

        public void Dispose()
        {
        }
    }
}
