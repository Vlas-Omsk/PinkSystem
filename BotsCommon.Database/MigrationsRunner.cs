using BotsCommon.Database.Entities;
using BotsCommon.Database.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace BotsCommon.Database
{
    public sealed class MigrationsRunner : IDisposable
    {
        private const string _versionPropertyName = "Version";

        private readonly DbContext _dbContext;
        private readonly IEnumerable<MigrationHistoryItem> _history;

        public MigrationsRunner(DbContext dbContext, IEnumerable<MigrationHistoryItem> history)
        {
            _dbContext = dbContext;
            _history = history;
        }

        ~MigrationsRunner()
        {
            Dispose();
        }

        public void Run()
        {
            var relationalConnection = _dbContext.GetService<IRelationalConnection>();
            var migrationsSqlGenerator = _dbContext.GetService<IMigrationsSqlGenerator>();

            Property versionProperty;
            try
            {
                versionProperty = _dbContext.Properties?.Find(_versionPropertyName);
            }
            catch
            {
                versionProperty = null;
            }

            var flag = versionProperty == null;

            if (flag)
                RunMigration(migrationsSqlGenerator, relationalConnection, MigrationType.Up, new InitialCreateMigration());

            MigrationHistoryItem lastItem = null;

            foreach (var item in _history)
            {
                lastItem = item;

                if (flag == false)
                {
                    if (item.Version == versionProperty.Value)
                        flag = true;

                    continue;
                }

                RunMigration(migrationsSqlGenerator, relationalConnection, item.Type, item.Migration);
            }

            if (versionProperty == null)
            {
                versionProperty = new Property()
                {
                    Name = _versionPropertyName,
                    Value = lastItem?.Version
                };
                _dbContext.Add(versionProperty);
            }
            else
            {
                versionProperty.Value = lastItem?.Version;
                _dbContext.Update(versionProperty);
            }

            _dbContext.SaveChanges();
        }

        private static void RunMigration(IMigrationsSqlGenerator migrationsSqlGenerator, IRelationalConnection relationalConnection, MigrationType type, Migration migration)
        {
            var operations = type switch
            {
                MigrationType.Up => migration.UpOperations,
                MigrationType.Down => migration.DownOperations,
                _ => throw new InvalidOperationException()
            };

            var commands = migrationsSqlGenerator.Generate(operations);

            foreach (var command in commands)
                command.ExecuteNonQuery(relationalConnection);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _dbContext.Dispose();
        }
    }
}
