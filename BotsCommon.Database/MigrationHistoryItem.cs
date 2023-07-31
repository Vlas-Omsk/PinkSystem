using Microsoft.EntityFrameworkCore.Migrations;

namespace BotsCommon.Database
{
    public sealed class MigrationHistoryItem
    {
        public MigrationHistoryItem(string version, Migration migration, MigrationType type)
        {
            Version = version;
            Migration = migration;
            Type = type;
        }

        public string Version { get; }
        public Migration Migration { get; }
        public MigrationType Type { get; }
    }
}
