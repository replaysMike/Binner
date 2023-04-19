using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddPartConsumedHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartsConsumed",
                schema: "dbo",
                table: "ProjectProduceHistory",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PartsConsumed",
                schema: "dbo",
                table: "ProjectPcbProduceHistory",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartsConsumed",
                schema: "dbo",
                table: "ProjectProduceHistory");

            migrationBuilder.DropColumn(
                name: "PartsConsumed",
                schema: "dbo",
                table: "ProjectPcbProduceHistory");
        }
    }
}
