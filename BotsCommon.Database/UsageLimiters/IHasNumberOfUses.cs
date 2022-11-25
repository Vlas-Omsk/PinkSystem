using System;

namespace BotsCommon.Database.UsageLimiters
{
    public interface IHasNumberOfUses
    {
        int NumberOfUses { get; set; }
    }
}
