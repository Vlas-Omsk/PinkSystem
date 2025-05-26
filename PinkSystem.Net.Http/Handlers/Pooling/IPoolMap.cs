namespace PinkSystem.Net.Http.Handlers.Pooling
{
    public interface IPoolMap
    {
        PoolSettings Settings { get; }

        PoolConnection RentConnection(IHttpRequestHandler handler);
        void BindExclusiveConnection(IHttpRequestHandler handler);
        void DisposeConnection(IHttpRequestHandler handler, bool ignoreNew);
    }
}
