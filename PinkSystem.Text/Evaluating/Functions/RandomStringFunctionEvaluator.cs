using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PinkSystem.Text.Evaluating.Functions
{
    public sealed class RandomStringFunctionEvaluator : IStringFunctionEvaluator
    {
        private static readonly string _numbers = "0123456789";
        private static readonly string _numbersWithoutZero = "123456789";
        private static readonly string _letters = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string _numbersAndLetters = _numbers + _letters;

        public bool TryEvaluate(IReadOnlyList<string> args, [NotNullWhen(true)] out string? result)
        {
            if (args[0] != "random")
            {
                result = null;
                return false;
            }

            if (args.Count != 4)
                throw new Exception("Function 'random' must provide 3 arguments");

            var (charset, charsetId) = args[1] switch
            {
                "numbers" => (_numbers, 0),
                "letters" => (_letters, 1),
                "numbersAndLetters" => (_numbersAndLetters, 2),
                _ => throw new Exception($"Unknown charset '{args[1]}'")
            };

            if (!int.TryParse(args[2], out var minLength))
                throw new Exception("Cannot parse minimum length");

            if (!int.TryParse(args[3], out var maxLength))
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

            result = string.Concat(chars);
            return true;
        }
    }
}
