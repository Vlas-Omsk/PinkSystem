using System;
using System.Collections.Generic;

namespace PinkSystem
{
    public sealed class RandomNextCharsOptions
    {
        private static readonly char[] _defaultAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        public IReadOnlyList<char> Alphabet { get; } = _defaultAlphabet;
        public static RandomNextCharsOptions Default { get; } = new();
    }

    public static class RandomExtensions
    {
        public static char[] NextChars(this Random self, int length, RandomNextCharsOptions? options = null)
        {
            options ??= new();

            var stringChars = new char[length];

            for (int i = 0; i < stringChars.Length; i++)
                stringChars[i] = options.Alphabet[self.Next(options.Alphabet.Count)];

            return stringChars;
        }

        public static string NextString(this Random self, int length, RandomNextCharsOptions? options = null)
        {
            return new string(NextChars(self, length, options));
        }

        public static bool NextBool(this Random self)
        {
            return self.Next(0, 2) == 0;
        }
    }
}
