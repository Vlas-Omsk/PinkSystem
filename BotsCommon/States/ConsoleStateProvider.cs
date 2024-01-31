using System.Text;

#nullable enable

namespace BotsCommon.States
{
    public sealed class ConsoleStateProvider : IStateProvider
    {
        private readonly string? _prefix;
        private readonly IConsoleWriter _writer;
        private readonly TimeSpan _updateInterval;
        private readonly Task? _task;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private StateContainer? _container;

        public ConsoleStateProvider(string? prefix, IConsoleWriter writer, TimeSpan updateInterval)
        {
            _prefix = prefix;
            _writer = writer;
            _updateInterval = updateInterval;

            _task = Task.Run(HandleUpdates);
        }

        public void Set(StateContainer container)
        {
            _container = container;

            if (_task?.IsFaulted == true)
                throw _task.Exception!;
        }

        private async Task HandleUpdates()
        {
            var lastCursorTop = 0;
            string? lastState = null;

            while (true)
            {
                if (_container != null)
                {
                    var builder = new StringBuilder();

                    foreach (var category in _container.Get().OrderBy(x => x.Key))
                    {
                        if (category.Key != null)
                            builder.Append(", " + category.Key + ": (");

                        builder.Append(category.Value);

                        if (category.Key != null)
                            builder.Append(") ");
                    }

                    var state = builder.ToString();

                    if (lastCursorTop != System.Console.CursorTop ||
                        lastState != state)
                        _writer.Write(state);

                    Thread.Sleep(100);

                    lastCursorTop = System.Console.CursorTop;
                    lastState = state;

                    if (_prefix != null)
                        System.Console.Title = $"{_prefix} | {state}";
                    else
                        System.Console.Title = state;
                }
                else if (_prefix != null)
                {
                    System.Console.Title = _prefix;
                }

                _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                try
                {
                    await Task.Delay(_updateInterval, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();

            try
            {
                _task?.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
