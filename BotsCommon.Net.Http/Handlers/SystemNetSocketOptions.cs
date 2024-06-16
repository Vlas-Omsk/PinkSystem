using System;
using System.Net.Http;
using System.Net.Sockets;

namespace BotsCommon.Net.Http.Handlers
{
    public sealed record SystemNetSocketOptions
    {
        public bool DisableLingering { get; set; }

        internal void Apply(SocketsHttpHandler handler)
        {
            if (DisableLingering)
            {
                handler.ConnectCallback = async (context, cancellationToken) =>
                {
                    var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
                    {
                        NoDelay = true,
                        LingerState = new(false, 0)
                    };

                    if (!OperatingSystem.IsLinux())
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                    try
                    {
                        await socket.ConnectAsync(context.DnsEndPoint, cancellationToken).ConfigureAwait(false);

                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                };
            }
        }
    }
}
