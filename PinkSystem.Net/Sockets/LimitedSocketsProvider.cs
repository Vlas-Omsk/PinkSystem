using System.Net.NetworkInformation;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using PinkSystem.Text;

namespace PinkSystem.Net.Sockets
{
    public sealed class LimitedSocketsProvider : ISocketsProvider
    {
        private readonly ISocketsProvider _socketsProvider;
        private readonly SemaphoreSlim _socketsLock;

        private sealed class LimitedSocket : ExtensionSocket
        {
            private readonly SemaphoreSlim _socketsLock;
            private int _disposed;

            public LimitedSocket(ISocket socket, SemaphoreSlim socketsLock) : base(socket)
            {
                _socketsLock = socketsLock;
            }

            public override void Dispose()
            {
                base.Dispose();

                if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                    return;

                _socketsLock.Release();
            }
        }

        public LimitedSocketsProvider(ISocketsProvider socketsProvider, int maxAvailableSockets)
        {
            _socketsProvider = socketsProvider;
            _socketsLock = new(maxAvailableSockets, maxAvailableSockets);

            MaxAvailableSockets = maxAvailableSockets;
        }

        public int MaxAvailableSockets { get; }
        public int CurrentAvailableSockets => _socketsLock.CurrentCount;

        public async Task<ISocket> Create(SocketType socketType, ProtocolType protocolType, CancellationToken cancellationToken)
        {
            await _socketsLock.WaitAsync(cancellationToken);

            var socket = new LimitedSocket(
                await _socketsProvider.Create(socketType, protocolType, cancellationToken),
                _socketsLock
            );

            return socket;
        }

        public static async Task<LimitedSocketsProvider> CreateDefault(ISocketsProvider socketsProvider, double percentOfAvailablePorts = 0.8)
        {
            var ephemeralPortRange = await GetEphemeralPortRange();
            var ephemeralPortAmount = ephemeralPortRange.End - ephemeralPortRange.Start;

            var activeEphemeralConnectionsAmount = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
                .Where(x => x.State != TcpState.Established && x.State != TcpState.Listen)
                .Count();

            // Reserving specified % of ephemeral ports, excluding active
            var availablePorts = (int)((ephemeralPortAmount - activeEphemeralConnectionsAmount) * percentOfAvailablePorts);

            return new(socketsProvider, availablePorts);
        }

        private static async Task<(int Start, int End)> GetEphemeralPortRange()
        {
            if (OperatingSystem.IsLinux())
            {
                var output = await ProcessUtils.GetProcessOutput("cat", "/proc/sys/net/ipv4/ip_local_port_range");

                var split = output.Trim().Split('\t', StringSplitOptions.RemoveEmptyEntries);

                if (split.Length != 2)
                    throw new Exception("Cannot parse sysctl output '" + output.Excape() + "'. Parts mismatch: " + string.Join(", ", split.Select(x => $"'{x}'")));

                return (int.Parse(split[0]), int.Parse(split[1]));
            }
            else if (OperatingSystem.IsWindows())
            {
                var output = await ProcessUtils.GetProcessOutput("netsh", "int ipv4 show dynamicport tcp");

                int startPort, numberOfPorts;

                using (var valueLines = output
                    .Split('\n')
                    .Where(x => x.Any(char.IsNumber))
                    .Select(x => int.Parse(x.Where(char.IsNumber).ToArray()))
                    .GetEnumerator())
                {
                    if (!valueLines.MoveNext())
                        throw new Exception("Cannot parse netsh output '" + output.Excape() + "'. Unexpected end");

                    startPort = valueLines.Current;

                    if (!valueLines.MoveNext())
                        throw new Exception("Cannot parse netsh output '" + output.Excape() + "'. Unexpected end");

                    numberOfPorts = valueLines.Current;
                }

                return (startPort, startPort + numberOfPorts);
            }
            else
            {
                return (ushort.MaxValue / 3, ushort.MaxValue);
            }
        }
    }
}
