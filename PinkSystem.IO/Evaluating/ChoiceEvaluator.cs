using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PinkSystem.IO.Evaluating
{
    public sealed class ChoiceEvaluator : IEvaluator
    {
        public bool TryEvaluate(IReadOnlyList<string> args, [NotNullWhen(true)] out string? result)
        {
            if (args[0] != "choice")
            {
                result = null;
                return false;
            }

            if (args.Count != 2)
                throw new Exception("Function 'choice' must provide atleast 1 arguments");

            result = args.Skip(Random.Shared.Next(1, args.Count)).First();
            return true;
        }
    }
}
