using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PinkSystem.IO.Evaluating
{
    public interface IEvaluator
    {
        bool TryEvaluate(IReadOnlyList<string> args, [NotNullWhen(true)] out string? result);
    }
}
