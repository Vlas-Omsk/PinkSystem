#nullable enable

namespace BotsCommon.States
{
    public interface IState
    {
        void Set(IEnumerable<KeyValuePair<string, string>> value);
    }
}
