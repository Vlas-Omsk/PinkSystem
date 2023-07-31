namespace BotsCommon
{
    public class LimitReachedException : Exception
    {
        public LimitReachedException() : base("Limit reached")
        {
        }

        public LimitReachedException(string message) : base(message)
        {
        }

        public LimitReachedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
