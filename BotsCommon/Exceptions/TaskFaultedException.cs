using System;

namespace BotsCommon.Exceptions
{
    public sealed class TaskFaultedException : Exception
    {
        public TaskFaultedException(Exception? innerException) : base("Task faulted", innerException)
        {
        }
    }
}
