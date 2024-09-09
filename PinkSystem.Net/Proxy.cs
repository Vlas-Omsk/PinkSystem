using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.RegularExpressions;

namespace PinkSystem.Net
{
    public sealed class Proxy
    {
        private string? _stringPresentation;
        private string? _uri;
        private string? _uriWithCredentials;

        public Proxy(ProxyScheme scheme, string host) : this(scheme, host, GetDefaultPort(scheme))
        {
            IsDefaultPort = true;
        }

        public Proxy(ProxyScheme scheme, string host, int port) : this(scheme, host, port, null, null)
        {
        }

        public Proxy(ProxyScheme scheme, string host, int port, string? username, string? password)
        {
            Scheme = scheme;
            Host = host;
            Port = port;
            Username = username;
            Password = password;
        }

        public ProxyScheme Scheme { get; }
        public string Host { get; }
        public int Port { get; }
        public string? Username { get; }
        public string? Password { get; }
        public bool IsDefaultPort { get; }
        public static Regex UserPasswordAtHostPortFormat { get; } = new("(?<username>.*?):(?<password>.*?)@(?<host>.*?):(?<port>.*)", RegexOptions.Compiled);
        public static Regex HostPortUserPasswordFormat { get; } = new("(?<host>.*?):(?<port>.*?):(?<username>.*?):(?<password>.*)", RegexOptions.Compiled);

        public bool HasCredentials => Username != null && Password != null;

        public WebProxy ToWebProxy()
        {
            var webProxy = new WebProxy(GetUri(false));

            if (HasCredentials)
                webProxy.Credentials = new NetworkCredential(Username, Password);

            return webProxy;
        }

        public string GetUri(bool useCredentials)
        {
            if (useCredentials)
            {
                if (_uriWithCredentials != null)
                    return _uriWithCredentials;
            }
            else
            {
                if (_uri != null)
                    return _uri;
            }

            var url = $"{GetSchemeName()}://";

            if (useCredentials && HasCredentials)
                url += $"{Username}:{Password}@";

            url += $"{Host}";

            if (!IsDefaultPort)
                url += $":{Port}";

            if (useCredentials)
                _uriWithCredentials = url;
            else
                _uri = url;

            return url;
        }

        public string GetSchemeName()
        {
            switch (Scheme)
            {
                case ProxyScheme.Http:
                    return "http";
                case ProxyScheme.Socks4:
                    return "socks4";
                case ProxyScheme.Socks5:
                    return "socks5";
                default:
                    throw new NotSupportedException();
            }
        }

        public override string ToString()
        {
            if (_stringPresentation != null)
                return _stringPresentation;

            var result = $"(Host: {Host}, Port: {Port}";

            if (HasCredentials)
                result += $", Username: {Username}, Password: {Password}";

            result += ")";

            _stringPresentation = result;

            return result;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (obj is Proxy proxy)
                return GetUri(useCredentials: true) == proxy.GetUri(useCredentials: true);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return GetUri(useCredentials: true).GetHashCode();
        }

        public static int GetDefaultPort(ProxyScheme scheme)
        {
            switch (scheme)
            {
                case ProxyScheme.Http:
                    return 80;
                case ProxyScheme.Socks4:
                    return 1080;
                case ProxyScheme.Socks5:
                    return 1080;
                default:
                    throw new NotSupportedException();
            }
        }

        public static Proxy Parse(string str, ProxyScheme scheme)
        {
            if (TryParse(str, scheme, UserPasswordAtHostPortFormat, out var proxy))
                return proxy;
            else if (TryParse(str, scheme, HostPortUserPasswordFormat, out proxy))
                return proxy;
            else
                throw new Exception("Cannot parse proxy using default formats");
        }

        public static bool TryParse(string str, ProxyScheme scheme, Regex format, [NotNullWhen(true)] out Proxy? proxy)
        {
            var match = format.Match(str);

            string? host = null;
            int? port = null;
            string? username = null;
            string? password = null;

            if (match.Groups.TryGetValue("host", out Group? hostGroup))
            {
                if (!hostGroup.Success)
                {
                    proxy = null;
                    return false;
                }

                host = hostGroup.Value;
            }
            if (match.Groups.TryGetValue("port", out Group? portGroup))
            {
                if (!portGroup.Success || !int.TryParse(portGroup.Value, out var parsedPort))
                {
                    proxy = null;
                    return false;
                }

                port = parsedPort;
            }
            if (match.Groups.TryGetValue("username", out Group? usernameGroup))
            {
                if (!usernameGroup.Success)
                {
                    proxy = null;
                    return false;
                }

                username = usernameGroup.Value;
            }
            if (match.Groups.TryGetValue("password", out Group? passwordGroup))
            {
                if (!passwordGroup.Success)
                {
                    proxy = null;
                    return false;
                }

                password = passwordGroup.ThrowIfNotSuccuess().Value;
            }

            proxy = new Proxy(
                scheme,
                host ?? throw new Exception("Host cannot be null"),
                port ?? GetDefaultPort(scheme),
                username,
                password
            );
            return true;
        }

        public static Proxy Parse(string str, ProxyScheme scheme, Regex format)
        {
            var match = format.Match(str);

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
                scheme,
                host ?? throw new Exception("Host cannot be null"),
                port ?? GetDefaultPort(scheme),
                username,
                password
            );
        }
    }
}
