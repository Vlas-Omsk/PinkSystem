using System;

namespace PinkSystem.Net.Http.Handlers.Pooling
{
    public interface IPool : IDisposable
    {
        PoolConnections Connections { get; }

        IPoolMap GetDefaultMap();
        IPoolMap GetMap(PoolSettings settings);
    }
}
