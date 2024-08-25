using System.Collections.Generic;

namespace PinkSystem.States
{
    public interface IState
    {
        void Set(IEnumerable<KeyValuePair<string, string>> value);
        void Change(string key, string value);
        void Remove(string key);
    }
}
