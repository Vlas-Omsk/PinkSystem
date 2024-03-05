using System;
using System.Net;

namespace BotsCommon.Net
{
    public sealed class Proxy
    {
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
            var url = $"{GetSchemeName()}://";

            if (useCredentials && HasCredentials)
                url += $"{Username}:{Password}@";

            url += $"{Host}";

            if (!IsDefaultPort)
                url += $":{Port}";

            return url;
        }

        private string GetSchemeName()
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
            var result = $"(Host: {Host}, Port: {Port}";

            if (HasCredentials)
                result += $", Username: {Username}, Password: {Password}";

            return result + ")";
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
    }
}
