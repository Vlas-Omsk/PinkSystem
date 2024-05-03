using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotsCommon
{
    public sealed class TasksPool : IDisposable, IAsyncDisposable
    {
        private readonly Task[] _tasks;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        public TasksPool(int count, CancellationToken cancellationToken = default)
        {
            _tasks = new Task[count];

            for (var i = 0; i < count; i++)
                _tasks[i] = Task.CompletedTask;

            _cancellationToken = cancellationToken;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        }

        ~TasksPool()
        {
            Dispose();
        }

        public int Count => _tasks.Length;

        public void StartNew(Func<Task> task)
        {
            StartNew(_ => task());
        }

        public void StartNew(Func<CancellationToken, Task> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var index = Array.FindIndex(_tasks, x => x.IsCompleted);

            if (index == -1)
                throw new Exception("All tasks are in progress");

            _tasks[index] = task(_cancellationTokenSource.Token);
        }

        public Task WaitAny()
        {
            return WaitAnyInternal();
        }

        public async Task<T?> WaitAnyAndGetValue<T>()
        {
            var task = await WaitAnyInternal().ConfigureAwait(false);

            if (task == Task.CompletedTask)
                return default;

            if (_cancellationTokenSource.Token.IsCancellationRequested && task.IsCanceled)
                return default;

            return ((Task<T>)task).Result;
        }

        private async Task<Task> WaitAnyInternal()
        {
            var task = await Task.WhenAny(_tasks).ConfigureAwait(false);

            if (task.IsFaulted)
                await WaitAll().ConfigureAwait(false);

            return task;
        }

        public async Task WaitAll()
        {
            try
            {
                await Task.WhenAll(_tasks).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
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
        }

        public async Task<IEnumerable<T>> WaitAllAndGetValues<T>()
        {
            await WaitAll().ConfigureAwait(false);

            return _tasks.Where(x => x is Task<T>).Select(x => ((Task<T>)x).Result);
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
