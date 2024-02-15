#nullable enable

namespace BotsCommon.States
{
    public interface IStateFactory : IDisposable
    {
        IState Create(string category);
        IState GetOrCreate(string category);
    }
}
