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

        public void WaitAny()
        {
            var index = Task.WaitAny(_tasks);

            UnwrapTask(_tasks[index]);
        }

        public void WaitAll()
        {
            Task.WaitAll(_tasks);
        }

        public void CancelAll()
        {
            _cancellationTokenSource.Cancel();

            WaitAll();

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            CancelAll();
        }

        private static void UnwrapTask(Task task)
        {
            if (task.IsFaulted)
                throw task.Exception;
        }
    }
}
