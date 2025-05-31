using PinkSystem.Net.Sockets;

namespace PinkSystem.Net
{
    public static class ISocketsProviderExtensions
    {
        public static ISocketsProvider WithStatisticsCollecting(this ISocketsProvider self, SocketStatisticsStorage storage)
        {
            return new StatisticsSocketsProvider(self, storage);
        }
    }
}
