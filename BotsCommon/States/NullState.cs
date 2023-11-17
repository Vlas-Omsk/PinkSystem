#nullable enable

namespace BotsCommon.States
{
    public sealed class NullState : IState
    {
        private NullState()
        {
        }

        public string? Value { get; } = null;

        public void Set(string value)
        {
        }

        public static NullState Instance { get; } = new();
    }
}
