using System;
using Microsoft.EntityFrameworkCore;

namespace BotsCommon.Database
{
    public abstract class DbContextFactory
    {
        private readonly DbContextOptions _options;

        public DbContextFactory(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<DbContext>();
            OnConfigure(builder);
            _options = builder.Options;
        }

        protected abstract void OnConfigure(DbContextOptionsBuilder<DbContext> builder);

        public DbContext Create()
        {
            return new DbContext(_options);
        }
    }
}
