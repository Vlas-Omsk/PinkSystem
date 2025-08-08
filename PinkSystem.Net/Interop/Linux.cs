using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace PinkSystem.Net.Interop
{
    internal static class Linux
    {
        // Constants from <sys/socket.h>
        private const int SOL_SOCKET = 1;
        private const int SO_BINDTODEVICE = 25;

        [DllImport("libc", SetLastError = true)]
        private static extern int setsockopt(int sockfd, int level, int optname, byte[] optval, uint optlen);

        public static void BindSocketToDevice(Socket socket, string ifName)
        {
            var ifNameBytes = Encoding.ASCII.GetBytes(ifName + "\0");

            var result = setsockopt(
                (int)socket.Handle,
                SOL_SOCKET,
                SO_BINDTODEVICE,
                ifNameBytes,
                (uint)ifNameBytes.Length
            );

            if (result != 0)
                throw new Win32Exception($"setsockopt(SO_BINDTODEVICE) failed with result {result}");
        }
    }
}
