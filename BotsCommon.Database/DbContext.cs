using BotsCommon.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace BotsCommon.Database
{
    public abstract class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Property> Properties { get; internal set; }
    }
}
