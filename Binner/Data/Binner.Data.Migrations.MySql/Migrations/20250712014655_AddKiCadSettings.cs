using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddKiCadSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KiCadSettingsJson",
                schema: "dbo",
                table: "OrganizationConfigurations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KiCadSettingsJson",
                schema: "dbo",
                table: "OrganizationConfigurations");
        }
    }
}
