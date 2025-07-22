using System.IO;
using System.Net;
using PinkSystem.Net.Sockets;

namespace PinkSystem.Net
{
    public static class ISocketExtensions
    {
        public static Stream GetStream(this ISocket self, IPEndPoint? remoteEndPoint = null, FileAccess access = FileAccess.ReadWrite, bool ownsSocket = true)
        {
            return new IO.NetworkStream(self, remoteEndPoint, access, ownsSocket);
        }
    }
}
