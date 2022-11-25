using System;

namespace BotsCommon.IO
{
    public class LimitReachedException : Exception
    {
        public LimitReachedException() : base("Limit reached")
        {
        }
    }
}
