using System.Diagnostics.CodeAnalysis;

namespace PinkSystem.IO.Evaluating
{
    public interface IEvaluator
    {
        bool TryEvaluate(string[] args, [NotNullWhen(true)] out string? result);
    }
}
