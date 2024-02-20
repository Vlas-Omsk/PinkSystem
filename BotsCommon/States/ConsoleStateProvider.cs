using System.Text;

#nullable enable

namespace BotsCommon.States
{
    public sealed class ConsoleStateProvider : IStateProvider
    {
        private readonly string? _prefix;
        private readonly IConsole _console;
        private readonly TimeSpan _updateInterval;
        private readonly Task? _task;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private StateContainer? _container;

        public ConsoleStateProvider(string? prefix, IConsole console, TimeSpan updateInterval)
        {
            _prefix = prefix;
            _console = console;
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
                        if (builder.Length > 0)
                            builder.Append(", ");

                        builder.Append(category.Key + ": (" + string.Join(", ", category.Value.Select(x => $"{x.Key}: {x.Value}")) + ")");
                    }

                    var state = builder.ToString();

                    if (lastCursorTop != System.Console.CursorTop ||
                        lastState != state)
                        _console.Write(state);

                    Thread.Sleep(100);

                    lastCursorTop = System.Console.CursorTop;
                    lastState = state;

                    if (_prefix != null)
                        _console.SetTitle($"{_prefix} | {state}");
                    else
                        _console.SetTitle(state);
                }
                else if (_prefix != null)
                {
                    _console.SetTitle(_prefix);
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
