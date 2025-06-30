using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using PinkSystem.Text.Evaluating.Functions;

namespace PinkSystem.Text.Evaluating
{
    public sealed class StringEvaluator
    {
        private static readonly Regex _functionRegex = new("{(.*?)}", RegexOptions.Compiled);
        private readonly ImmutableArray<IStringFunctionEvaluator> _functionEvaluators;

        public StringEvaluator(IEnumerable<IStringFunctionEvaluator> functionEvaluators)
        {
            _functionEvaluators = functionEvaluators.ToImmutableArray();
        }

        public string Evaluate(string str)
        {
            return _functionRegex.Replace(str, x =>
            {
                var args = x.Groups[1].Value.Split(',');

                foreach (var function in _functionEvaluators)
                {
                    if (function.TryEvaluate(args, out var result))
                        return result;
                }

                throw new Exception($"Function '{args[0]}' not supported");
            });
        }

        public static StringEvaluator Default => new StringEvaluator(
            [
                new RandomStringFunctionEvaluator(),
                new ChoiceStringFunctionEvaluator()
            ]
        );
    }
}
