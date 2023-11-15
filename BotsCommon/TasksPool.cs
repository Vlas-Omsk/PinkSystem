namespace BotsCommon
{
    public sealed class TasksPool : IDisposable
    {
        private readonly Task[] _tasks;
        private readonly object _lock = new object();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public TasksPool(int count)
        {
            _tasks = new Task[count];

            for (var i = 0; i < count; i++)
                _tasks[i] = Task.CompletedTask;
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

            lock (_lock)
            {
                var index = Array.FindIndex(_tasks, x => x.IsCompleted);

                if (index == -1)
                    throw new Exception("All tasks are in progress");

                _tasks[index] = task(_cancellationTokenSource.Token);
            }
        }

        public Task WaitAny()
        {
            return WaitAnyInternal();
        }

        public async Task<T> WaitAnyAndGetValue<T>()
        {
            var task = await WaitAnyInternal();

            if (task == Task.CompletedTask)
                return default;

            return ((Task<T>)task).Result;
        }

        private async Task<Task> WaitAnyInternal()
        {
            var task = await Task.WhenAny(_tasks);

            UnwrapTask(task);

            return task;
        }

        public Task WaitAll()
        {
            return Task.WhenAll(_tasks);
        }

        public async Task CancelAll()
        {
            _cancellationTokenSource.Cancel();

            try
            {
                await WaitAll();
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(x => x is OperationCanceledException))
            {
            }
            catch (OperationCanceledException)
            {
            }

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            try
            {
                CancelAll().Wait();
            }
            catch
            {
            }
        }

        private static void UnwrapTask(Task task)
        {
            if (task.IsFaulted)
                throw task.Exception;
        }
    }
}
