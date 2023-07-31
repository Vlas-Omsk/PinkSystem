using Microsoft.EntityFrameworkCore.Migrations;

namespace BotsCommon.Database.Migrations
{
    internal sealed class InitialCreateMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Properties",
                columns: x => new
                {
                    Name = x.Column<string>(),
                    Value = x.Column<string>(nullable: true)
                },
                constraints: x =>
                {
                    x.PrimaryKey(
                        name: "PK_PropertiesName",
                        columns: x => x.Name
                    );
                }
            );
        }
    }
}
