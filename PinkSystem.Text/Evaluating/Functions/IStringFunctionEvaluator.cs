using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PinkSystem.Text.Evaluating.Functions
{
    public interface IStringFunctionEvaluator
    {
        bool TryEvaluate(IReadOnlyList<string> args, [NotNullWhen(true)] out string? result);
    }
}
