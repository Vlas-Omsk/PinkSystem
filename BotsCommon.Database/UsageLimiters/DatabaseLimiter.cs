using System;
using BotsCommon.IO;

namespace BotsCommon.Database.UsageLimiters
{
    public abstract class DatabaseUsageLimiter<TEntity, T> : IUsageLimiter<T>, IDisposable where TEntity : class, IHasNumberOfUses
    {
        private readonly DbContext _dbContext;

        protected DatabaseUsageLimiter(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        ~DatabaseUsageLimiter()
        {
            Dispose();
        }

        public int GetNumberOfUses(T item)
        {
            var entity = FindEntity(item);

            return entity.NumberOfUses;
        }

        public void IncreaseNumberOfUses(T item)
        {
            var entity = FindEntity(item);

            entity.NumberOfUses++;

            _dbContext.Update(entity);
            _dbContext.SaveChanges();
        }

        protected abstract TEntity FindEntity(T item);

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _dbContext.Dispose();
        }
    }
}
