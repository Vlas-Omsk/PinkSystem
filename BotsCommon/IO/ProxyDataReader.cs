using System.Text.RegularExpressions;
using PinkNet;

namespace BotsCommon.IO
{
    public sealed class ProxyDataReader : IDataReader<Proxy>
    {
        private readonly IDataReader<string> _reader;

        public ProxyDataReader(IDataReader<string> reader)
        {
            _reader = reader;
        }

        public ProxyScheme ProxyScheme { get; set; } = ProxyScheme.Http;
        public Regex Format { get; set; } = new Regex(@"((?<username>.*?):(?<password>.*?)@)?(((?<host>.*?):(?<port>.*?))|(?<host>.*?))$", RegexOptions.Compiled);
        public int Length => _reader.Length;
        public int Index => _reader.Index;

        public Proxy Read()
        {
            var match = Format.Match(_reader.Read());

            string host = null;
            int? port = null;
            string username = null;
            string password = null;

            if (match.Groups.TryGetValue("host", out Group hostGroup))
                host = hostGroup.Value;
            if (match.Groups.TryGetValue("port", out Group portGroup))
                port = portGroup.Success ? int.Parse(portGroup.Value) : null;
            if (match.Groups.TryGetValue("username", out Group usernameGroup))
                username = usernameGroup.Success ? usernameGroup.Value : null;
            if (match.Groups.TryGetValue("password", out Group passwordGroup))
                password = passwordGroup.Success ? passwordGroup.Value : null;

            return new Proxy(ProxyScheme, host, port ?? Proxy.GetDefaultPort(ProxyScheme), username, password);
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
