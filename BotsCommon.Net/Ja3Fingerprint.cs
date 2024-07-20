using Org.BouncyCastle.Tls;
using System;

namespace BotsCommon.Net
{
    public sealed record Ja3Fingerprint(
        ProtocolVersion[] SupportedVersions,
        int[] SupportedCiphers,
        int[] SupportedGroups,
        int[] ExtensionsOrder
    )
    {
        public static Ja3Fingerprint Parse(string fingerprints)
        {
            var parts = fingerprints.Split(',');

            if (parts.Length != 5)
                throw new FormatException("JA3 isn't in correct format");

            var tlsVersion = short.Parse(parts[0]);
            var ciphers = parts[1].Split('-').Select(x => int.Parse(x)).ToArray();
            var extensions = parts[2].Split('-').Select(x => int.Parse(x)).ToArray();
            var ellipticCurve = parts[3].Split('-').Select(x => int.Parse(x)).ToArray();

            var majorTlsVersion = (tlsVersion & 0b1111111100000000) >> 8;
            var minorTlsVersion = tlsVersion & 0b0000000011111111;

            return new Ja3Fingerprint(
                [ProtocolVersion.Get(majorTlsVersion, minorTlsVersion)],
                ciphers,
                ellipticCurve,
                extensions
            );
        }
    }
}
