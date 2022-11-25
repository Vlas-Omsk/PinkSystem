using System;
using BotsCommon.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace BotsCommon.Database
{
    public sealed class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Property> Properties { get; private set; }
    }
}
