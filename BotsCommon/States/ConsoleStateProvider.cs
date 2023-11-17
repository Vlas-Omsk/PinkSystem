using System.Text;

#nullable enable

namespace BotsCommon.States
{
    public sealed class ConsoleStateProvider : IStateProvider
    {
        private readonly string? _prefix;
        private readonly Task? _task;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private string? _state;

        public ConsoleStateProvider(string? prefix, Action<string> callback)
        {
            _prefix = prefix;

            if (_prefix != null)
                Console.Title = _prefix;

            _task = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    if (_state != null)
                        callback(_state);

                    await Task.Delay(5000);
                }
            });
        }

        public void Set(StateContainer container)
        {
            var builder = new StringBuilder();

            foreach (var category in container.Get())
            {
                if (category.Key != null)
                    builder.Append(category.Key + ": (");

                builder.Append(category.Value);

                if (category.Key != null)
                    builder.Append(") ");
            }

            _state = builder.ToString();

            if (_prefix != null)
                Console.Title = $"{_prefix} | {_state}";
            else
                Console.Title = _state;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _task?.GetAwaiter().GetResult();
        }
    }
}
