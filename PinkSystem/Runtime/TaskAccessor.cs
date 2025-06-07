using System.Threading.Tasks;

namespace PinkSystem.Runtime
{
    public sealed class TaskAccessor
    {
        private readonly TaskType _type;
        private readonly object _task;

        private enum TaskType
        {
            VoidValueTask,
            ValueTask,
            VoidTask,
            Task
        }

        public TaskAccessor(object instance)
        {
            _task = instance;

            if (_task.GetType().GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                _type = TaskType.ValueTask;
            }
            else if (_task is ValueTask valueTask)
            {
                _type = TaskType.VoidValueTask;
            }
            else if (_task.GetType().GetGenericTypeDefinition() == typeof(Task<>))
            {
                _type = TaskType.Task;
            }
            else if (_task is Task)
            {
                _type = TaskType.VoidTask;
            }
            else
            {
                throw new System.Exception("Unknown task type");
            }
        }

        public async Task<object?> Wait()
        {
            if (_type == TaskType.VoidValueTask)
            {
                await (ValueTask)_task;

                return null;
            }
            else if (_type == TaskType.ValueTask)
            {
                var task = (Task)_task.AccessObject().CallMethod("AsTask")!;

                await task;

                return task.AccessObject().GetProperty("Result");
            }
            else if (_type == TaskType.VoidTask)
            {
                await (Task)_task;

                return null;
            }
            else if (_type == TaskType.Task)
            {
                var task = (Task)_task;

                await task;

                return task.AccessObject().GetProperty("Result");
            }
            else
            {
                throw new System.Exception("Unknown task type");
            }
        }
    }
}
