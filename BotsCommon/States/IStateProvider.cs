#nullable enable

namespace BotsCommon.States
{
    public interface IStateProvider : IDisposable
    {
        void Set(StateContainer container);
    }
}
