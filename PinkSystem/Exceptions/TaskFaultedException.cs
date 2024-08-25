using System;

namespace PinkSystem.Exceptions
{
    public sealed class TaskFaultedException : Exception
    {
        public TaskFaultedException(Exception? innerException) : base("Task faulted", innerException)
        {
        }
    }
}
