using BotsCommon.Net;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace BotsCommon.IO.Data
{
    public sealed class ProxyDataReader : IDataReader<Proxy>
    {
        private static readonly Regex _functionsRegex = new("{(.*?)}", RegexOptions.Compiled);
        private static readonly string _numbers = "0123456789";
        private static readonly string _numbersWithoutZero = "123456789";
        private static readonly string _letters = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string _numbersAndLetters = _numbers + _letters;
        private readonly IDataReader<string> _reader;
        private int _index;

        public ProxyDataReader(IDataReader<string> reader, ProxyScheme scheme, Regex format)
        {
            _reader = reader;
            Scheme = scheme;
            Format = format;
        }

        public ProxyScheme Scheme { get; set; }
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
                data = _functionsRegex.Replace(data, x =>
                {
                    var parts = x.Groups[1].Value.Split(',');

                    if (parts[0] == "random")
                    {
                        if (parts.Length != 4)
                            throw new Exception("Function 'random' must provide 3 arguments");

                        var (charset, charsetId) = parts[1] switch
                        {
                            "numbers" => (_numbers, 0),
                            "letters" => (_letters, 1),
                            "numbersAndLetters" => (_numbersAndLetters, 2),
                            _ => throw new Exception($"Unknown charset '{parts[1]}'")
                        };

                        if (!int.TryParse(parts[2], out var minLength))
                            throw new Exception("Cannot parse minimum length");

                        if (!int.TryParse(parts[3], out var maxLength))
                            throw new Exception("Cannot parse maximum length");

                        if (minLength > maxLength)
                            throw new Exception("Maximum length cannot be less than minimum length");

                        var chars = Enumerable.Range(0, Random.Shared.Next(minLength, maxLength + 1))
                            .Select(x => charset[Random.Shared.Next(charset.Length)])
                            .Select((x, index) =>
                            {
                                if (index == 0 && charsetId == 0 && x == '0')
                                    return _numbersWithoutZero[Random.Shared.Next(_numbersWithoutZero.Length)];

                                return x;
                            });

                        return string.Concat(chars);
                    }
                    else
                    {
                        throw new Exception($"Function '{parts[0]}' not supported");
                    }
                });

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
