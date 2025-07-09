using PinkSystem.Exceptions;
using PinkSystem.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PinkSystem
{
    public sealed class TasksPool : IDisposable, IAsyncDisposable
    {
        private readonly TasksPoolCore _tasksPool;

        public TasksPool(int count, CancellationToken cancellationToken = default)
        {
            _tasksPool = new(count, cancellationToken);
        }

        public int Count => _tasksPool.Count;

        public void StartNew(Func<Task> task)
        {
            _tasksPool.StartNew(task);
        }

        public void StartNew(Func<CancellationToken, Task> task)
        {
            _tasksPool.StartNew(task);
        }

        public async Task WaitAny()
        {
            await _tasksPool.WaitAny();
        }

        public async Task WaitAll()
        {
            await _tasksPool.WaitAll();
        }

        public async Task CancelAll()
        {
            await _tasksPool.CancelAll();
        }

        public void Dispose()
        {
            _tasksPool.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _tasksPool.DisposeAsync();
        }
    }

    public sealed class TasksPool<T> : IDisposable, IAsyncDisposable
    {
        private readonly TasksPoolCore _tasksPool;

        public TasksPool(int count, CancellationToken cancellationToken = default)
        {
            _tasksPool = new(count, cancellationToken);
        }

        public int Count => _tasksPool.Count;

        public void StartNew(Func<Task<T>> task)
        {
            _tasksPool.StartNew(task);
        }

        public void StartNew(Func<CancellationToken, Task<T>> task)
        {
            _tasksPool.StartNew(task);
        }

        public async Task<T?> WaitAny()
        {
            var result = await _tasksPool.WaitAny().ConfigureAwait(false);

            if (result == null)
                return default;

            return (T?)result;
        }

        public async Task<IEnumerable<T?>> WaitAll()
        {
            return (await _tasksPool.WaitAll().ConfigureAwait(false))
                .Select(x =>
                {
                    if (x == null)
                        return default;

                    return (T?)x;
                });
        }

        public async Task CancelAll()
        {
            await _tasksPool.CancelAll();
        }

        public void Dispose()
        {
            _tasksPool.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _tasksPool.DisposeAsync();
        }
    }

    internal sealed class TasksPoolCore : IDisposable, IAsyncDisposable
    {
        private readonly Slot[] _slots;
        private readonly Queue<int> _freeIndexes = new();
        private readonly Queue<int> _completedIndexes = new();
        private readonly object _lock = new();
        private CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;
        private int _disposed;

        private sealed class Slot
        {
            public Task Task { get; set; } = null!;
            public object? Result { get; set; }
            public Exception? Exception { get; set; }
        }

        public TasksPoolCore(int count, CancellationToken cancellationToken)
        {
            Count = count;

            _slots = Enumerable.Range(0, count).Select(_ => new Slot()
            {
                Task = Task.CompletedTask
            }).ToArray();
            _freeIndexes = new(Enumerable.Range(0, count));

            _cancellationToken = cancellationToken;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        }

        ~TasksPoolCore()
        {
            Dispose();
        }

        public int Count { get; }

        public void StartNew(Func<Task> func)
        {
            ArgumentNullException.ThrowIfNull(func);

            StartNew(_ => func());
        }

        public void StartNew(Func<CancellationToken, Task> func)
        {
            ArgumentNullException.ThrowIfNull(func);

            ThrowIfDisposedOrCancelled();

            lock (_lock)
            {
                if (!_freeIndexes.TryDequeue(out var freeIndex))
                    throw new InvalidOperationException("All slots busy");

                var slot = new Slot();

                slot.Task = StartTask(func, slot, freeIndex, _cancellationTokenSource.Token);

                _slots[freeIndex] = slot;
            }
        }

        private async Task StartTask(Func<CancellationToken, Task> func, Slot item, int index, CancellationToken cancellationToken)
        {
            Task? task = null;

            try
            {
                task = func(cancellationToken);

                await task;

                var taskType = task.GetType();

                if (taskType.IsGenericType)
                    item.Result = new ObjectAccessor(task, taskType).GetProperty("Result");
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested && task?.IsCanceled == true)
                    item.Exception = new TaskCanceledException("Task cancelled", ex, cancellationToken);
                else
                    item.Exception = new TaskFaultedException(ex);
            }

            lock (_lock)
            {
                _completedIndexes.Enqueue(index);
            }
        }

        public async Task<object?> WaitAny()
        {
            ThrowIfDisposedOrCancelled();

            var slot = await WaitAnySlot();

            if (slot == null)
                return null;

            if (slot.Exception != null)
                await WaitAllInternal([slot]);

            return slot.Result;
        }

        private async Task<Slot?> WaitAnySlot()
        {
            while (true)
            {
                int completedIndex;
                ImmutableArray<Task>? tasks = null;
                Slot? slot = null;

                lock (_lock)
                {
                    if (_freeIndexes.Count > 0)
                        return null;

                    if (_completedIndexes.TryDequeue(out completedIndex))
                    {
                        slot = _slots[completedIndex];

                        _freeIndexes.Enqueue(completedIndex);
                    }
                    else
                    {
                        completedIndex = -1;

                        tasks = _slots.Select(x => x!.Task).ToImmutableArray();
                    }
                }

                if (completedIndex == -1)
                {
                    await Task.WhenAny(tasks!.Value);
                    continue;
                }

                return slot!;
            }
        }

        public Task<IEnumerable<object?>> WaitAll()
        {
            ThrowIfDisposedOrCancelled();

            return WaitAllInternal([]);
        }

        private async Task<IEnumerable<object?>> WaitAllInternal(IEnumerable<Slot> externallyCompletedSlots)
        {
            var exceptions = new List<Exception>();
            var results = new List<object?>();

            foreach (var item in externallyCompletedSlots)
            {
                if (item.Exception != null)
                    exceptions.Add(item.Exception);

                results.Add(item.Result);
            }

            await foreach (var slot in WaitAllSlots())
            {
                if (slot.Exception != null)
                    exceptions.Add(slot.Exception);

                results.Add(slot.Result);
            }

            var nonCancelationExceptions = exceptions.Where(x => x is not TaskCanceledException).ToImmutableArray();

            if (nonCancelationExceptions.Length > 1)
                throw new AggregateException("Exception ocurrend when executing tasks on pool", nonCancelationExceptions);
            else if (nonCancelationExceptions.Length == 1)
                throw new Exception("Exception ocurrend when executing task on pool", nonCancelationExceptions[0]);

            return results;
        }

        private async IAsyncEnumerable<Slot> WaitAllSlots()
        {
            while (true)
            {
                bool hasBusyItems = false;
                ImmutableArray<Task>? tasks = null;
                Slot? slot = null;

                lock (_lock)
                {
                    while (_completedIndexes.TryDequeue(out var completedIndex))
                    {
                        slot = _slots[completedIndex];

                        _freeIndexes.Enqueue(completedIndex);

                        yield return slot;
                    }

                    hasBusyItems = _freeIndexes.Count != Count;

                    tasks = hasBusyItems ?
                        _slots.Select(x => x!.Task).ToImmutableArray() :
                        null;
                }

                if (hasBusyItems)
                {
                    await Task.WhenAll(tasks!.Value);
                    continue;
                }

                break;
            }
        }

        public async Task CancelAll()
        {
            ThrowIfDisposedOrCancelled();

            await CancelAllOnce().ConfigureAwait(false);

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        }

        private async Task CancelAllOnce()
        {
            _cancellationTokenSource.Cancel();

            await WaitAllInternal([]).ConfigureAwait(false);

            _cancellationTokenSource.Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            CancelAllOnce().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);

            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            await CancelAllOnce().ConfigureAwait(false);
        }

        private void ThrowIfDisposedOrCancelled()
        {
            ObjectDisposedException.ThrowIf(_disposed == 1, this);

            if (_cancellationTokenSource.IsCancellationRequested)
                throw new OperationCanceledException("Cancellation was requested", _cancellationToken);
        }
    }
}
