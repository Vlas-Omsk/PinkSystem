﻿using PinkSystem.Exceptions;
using PinkSystem.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            var task = await _tasksPool.WaitAny().ConfigureAwait(false);

            if (task == Task.CompletedTask)
                return default;

            if (_tasksPool.IsCancelled && task.IsCanceled)
                return default;

            return (T?)new ObjectAccessor(task, task.GetType()).GetProperty("Result");
        }

        public async Task<IEnumerable<T?>> WaitAll()
        {
            return (await _tasksPool.WaitAll().ConfigureAwait(false))
                .Where(x => x != Task.CompletedTask)
                .Select(x => (T?)new ObjectAccessor(x, x.GetType()).GetProperty("Result"));
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
        private readonly Task[] _tasks;
        private readonly Queue<int> _emptyIndexes;
        private readonly ConcurrentQueue<(int, Task)> _completedTasks;
        private readonly SemaphoreSlim _taskCompletedEvent;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        public TasksPoolCore(int count, CancellationToken cancellationToken)
        {
            Count = count;

            _completedTasks = new(
                Enumerable.Range(0, count).Select(x => (x, Task.CompletedTask))
            );
            _tasks = _completedTasks.Select(x => x.Item2).ToArray();
            _emptyIndexes = new(
                _completedTasks.Select(x => x.Item1)
            );
            _taskCompletedEvent = new(count, count);

            _cancellationToken = cancellationToken;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        }

        ~TasksPoolCore()
        {
            Dispose();
        }

        public int Count { get; }
        public bool IsCancelled => _cancellationTokenSource.IsCancellationRequested;

        public void StartNew(Func<Task> func)
        {
            StartNew(_ => func());
        }

        public void StartNew(Func<CancellationToken, Task> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (!_emptyIndexes.TryDequeue(out var index))
                throw new Exception("All tasks are in progress");

            var task = StartNewTask(func);

            _tasks[index] = task;

            task.ContinueWith((_) =>
            {
                _completedTasks.Enqueue((index, task));

                _taskCompletedEvent.Release();
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        private async Task<object?> StartNewTask(Func<CancellationToken, Task> func)
        {
            var task = func(_cancellationTokenSource.Token);

            try
            {
                await task;
            }
            catch (Exception ex)
            {
                if (_cancellationTokenSource.IsCancellationRequested && task.IsCanceled)
                    throw new TaskCanceledException("Task cancelled", ex, _cancellationTokenSource.Token);
                    
                throw new TaskFaultedException(ex);
            }

            var taskType = task.GetType();

            if (taskType.IsGenericType)
                return new ObjectAccessor(task, taskType).GetProperty("Result");

            return null;
        }

        public async Task<Task> WaitAny()
        {
            await _taskCompletedEvent.WaitAsync();

            if (!_completedTasks.TryDequeue(out var tuple))
                throw new InvalidOperationException("Completed tasks collection empty");

            _emptyIndexes.Enqueue(tuple.Item1);

            if (tuple.Item2.IsFaulted)
                await WaitAll().ConfigureAwait(false);

            return tuple.Item2;
        }

        public async Task<IEnumerable<Task>> WaitAll()
        {
            try
            {
                await Task.WhenAll(_tasks).ConfigureAwait(false);

                return _tasks;
            }
            catch
            {
                var exceptions = _tasks
                    .Where(t => t.Exception != null)
                    .Select(t => t.Exception!)
                    .ToArray();

                if (exceptions.Length == 1)
                    throw new Exception("Exception ocurrend when executing task on pool", exceptions[0]);
                else
                    throw new AggregateException("Exception ocurrend when executing task on pool", exceptions);
            }
            finally
            {
                _completedTasks.Clear();
            }
        }

        public async Task CancelAll()
        {
            await CancelAllInternal().ConfigureAwait(false);

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        }

        private async Task CancelAllInternal()
        {
            _cancellationTokenSource.Cancel();

            try
            {
                await WaitAll().ConfigureAwait(false);
            }
            catch (Exception ex) when (ex.CheckAll(x => x is OperationCanceledException))
            {
            }

            _cancellationTokenSource.Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            CancelAllInternal().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);

            await CancelAllInternal().ConfigureAwait(false);
        }
    }
}
