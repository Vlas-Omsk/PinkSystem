using System;

namespace PinkSystem.States
{
    public interface IStateFactory : IDisposable
    {
        IState Create(string category);
        IState GetOrCreate(string category);
    }
}
