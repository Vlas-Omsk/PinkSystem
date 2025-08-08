using System.Threading.Tasks;
using PinkSystem.Runtime;

namespace PinkSystem
{
    public static class RuntimeExtensions
    {
        public static ObjectAccessor AccessObject(this object self)
        {
            return new ObjectAccessor(self);
        }

        public static TaskAccessor AccessTask(this object self)
        {
            return new TaskAccessor(self);
        }

        public static async Task<object?> AsTask(this object self)
        {
            var accessor = new TaskAccessor(self);

            return await accessor.Wait();
        }

        public static async Task<T> AsTask<T>(this object self)
        {
            var accessor = new TaskAccessor(self);

            return (T)(await accessor.Wait())!;
        }
    }
}
