using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddExtraConfigFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CacheAbsoluteExpirationMinutes",
                schema: "dbo",
                table: "OrganizationConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CacheSlidingExpirationMinutes",
                schema: "dbo",
                table: "OrganizationConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxCacheItems",
                schema: "dbo",
                table: "OrganizationConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UseModule",
                schema: "dbo",
                table: "OrganizationConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CacheAbsoluteExpirationMinutes",
                schema: "dbo",
                table: "OrganizationConfigurations");

            migrationBuilder.DropColumn(
                name: "CacheSlidingExpirationMinutes",
                schema: "dbo",
                table: "OrganizationConfigurations");

            migrationBuilder.DropColumn(
                name: "MaxCacheItems",
                schema: "dbo",
                table: "OrganizationConfigurations");

            migrationBuilder.DropColumn(
                name: "UseModule",
                schema: "dbo",
                table: "OrganizationConfigurations");
        }
    }
}
