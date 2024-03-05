using System.Collections.Generic;

namespace BotsCommon.States
{
    public interface IState
    {
        void Set(IEnumerable<KeyValuePair<string, string>> value);
    }
}
