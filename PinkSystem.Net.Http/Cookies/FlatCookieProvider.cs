using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using PinkSystem.IO.Content;

namespace PinkSystem.Net.Http.Cookies
{
    public sealed class FlatCookieProvider : ICookieProvider
    {
        public FlatCookieProvider(string domain)
        {
            Domain = domain;
        }

        public string Domain { get; }

        public bool IsSupported(IContentReader reader)
        {
            using var streamReader = new StreamReader(reader.CreateStream());

            var line = streamReader.ReadLine();

            if (line == null)
                return false;

            line = streamReader.ReadLine();

            if (line != null)
                return false;

            return true;
        }

        public IEnumerable<Cookie> ReadAll(IContentReader reader)
        {
            using var streamReader = new StreamReader(reader.CreateStream());

            var line = streamReader.ReadLine() ?? throw new Exception("Line cannot be null");

            foreach (var cookie in line.Split(';'))
            {
                var trimmedCookie = cookie.Trim();

                var splitIndex = trimmedCookie.IndexOf('=');

                if (splitIndex == -1)
                    throw new Exception("Incorrect cookies string format");

                yield return new Cookie(
                    trimmedCookie[..splitIndex],
                    trimmedCookie[(splitIndex + 1)..]
                )
                {
                    Domain = Domain
                };
            }
        }
    }
}
