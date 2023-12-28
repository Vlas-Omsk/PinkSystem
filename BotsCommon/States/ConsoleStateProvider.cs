using System.Text;
using System.Threading;

#nullable enable

namespace BotsCommon.States
{
    public sealed class ConsoleStateProvider : IStateProvider
    {
        private readonly string? _prefix;
        private readonly Task? _task;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private StateContainer? _container;

        public ConsoleStateProvider(string? prefix, Action<string> callback)
        {
            _prefix = prefix;

            _task = Task.Run(async () =>
            {
                while (true)
                {
                    if (_container != null)
                    {
                        // Required for thread safety.
                        var container = _container;

                        var builder = new StringBuilder();

                        foreach (var category in container.Get().OrderBy(x => x.Key))
                        {
                            if (category.Key != null)
                                builder.Append(", " + category.Key + ": (");

                            builder.Append(category.Value);

                            if (category.Key != null)
                                builder.Append(") ");
                        }

                        var state = builder.ToString();

                        callback(state);

                        if (_prefix != null)
                            Console.Title = $"{_prefix} | {state}";
                        else
                            Console.Title = state;
                    }
                    else if (_prefix != null)
                    {
                        Console.Title = _prefix;
                    }

                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    try
                    {
                        await Task.Delay(1000, _cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            });
        }

        public void Set(StateContainer container)
        {
            _container = container;

            if (_task?.IsFaulted == true)
                throw _task.Exception!;
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
