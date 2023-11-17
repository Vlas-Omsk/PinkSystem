#nullable enable

namespace BotsCommon.States
{
    public sealed class State : IState
    {
        private readonly StateFactory _factory;

        public State(StateFactory factory)
        {
            _factory = factory;
        }

        public string? Value { get; private set; }

        public void Set(string value)
        {
            Value = value;
            _factory.NotifyUpdate();
        }
    }
}
