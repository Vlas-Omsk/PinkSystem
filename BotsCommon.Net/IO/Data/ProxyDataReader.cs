using BotsCommon.Net;
using System;
using System.Text.RegularExpressions;

namespace BotsCommon.IO.Data
{
    public sealed class ProxyDataReader : IDataReader<Proxy>
    {
        private readonly IDataReader<string> _reader;

        public ProxyDataReader(IDataReader<string> reader, ProxyScheme scheme, Regex format)
        {
            _reader = reader;
            Scheme = scheme;
            Format = format;
        }

        public ProxyScheme Scheme { get; set; }
        public Regex Format { get; set; }
        public int? Length => _reader.Length;
        public int Index => _reader.Index;

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
