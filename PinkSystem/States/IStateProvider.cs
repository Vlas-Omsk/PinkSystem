using System;

namespace PinkSystem.States
{
    public interface IStateProvider : IDisposable
    {
        void Set(StateContainer container);
    }
}
