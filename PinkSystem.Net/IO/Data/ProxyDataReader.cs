using PinkSystem.Net;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace PinkSystem.IO.Data
{
    public sealed class ProxyDataReader : IDataReader<Proxy>
    {
        private readonly IDataReader<string> _reader;
        private int _index;

        public ProxyDataReader(IDataReader<string> reader, ProxyProtocol scheme, Regex format)
        {
            _reader = reader;
            Scheme = scheme;
            Format = format;
        }

        public ProxyProtocol Scheme { get; set; }
        public Regex Format { get; set; }
        public int? Length { get; } = null;
        public int Index => _index;

        public Proxy? Read()
        {
            var data = _reader.Read();

            if (data == null)
                return null;

            try
            {
                var match = Format.Match(data);

                string? host = null;
                int? port = null;
                string? username = null;
                string? password = null;

                if (match.Groups.TryGetValue("host", out Group? hostGroup))
                    host = hostGroup.ThrowIfNotSuccuess().Value;
                if (match.Groups.TryGetValue("port", out Group? portGroup))
                    port = int.Parse(portGroup.ThrowIfNotSuccuess().Value);
                if (match.Groups.TryGetValue("username", out Group? usernameGroup))
                    username = usernameGroup.ThrowIfNotSuccuess().Value;
                if (match.Groups.TryGetValue("password", out Group? passwordGroup))
                    password = passwordGroup.ThrowIfNotSuccuess().Value;

                Interlocked.Increment(ref _index);

                return new Proxy(
                    Scheme,
                    host ?? throw new Exception("Host cannot be null"),
                    port ?? Proxy.GetDefaultPort(Scheme),
                    username,
                    password
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Proxy was not in correct format '{data}'", ex);
            }
        }

        object? IDataReader.Read()
        {
            return Read();
        }

        public void Reset()
        {
            _reader.Reset();
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
