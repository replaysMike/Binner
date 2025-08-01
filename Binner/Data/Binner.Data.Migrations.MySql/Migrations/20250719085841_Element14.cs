using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class Element14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Element14PartNumber",
                schema: "dbo",
                table: "Parts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Element14ApiKey",
                schema: "dbo",
                table: "OrganizationIntegrationConfigurations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Element14ApiUrl",
                schema: "dbo",
                table: "OrganizationIntegrationConfigurations",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Element14Country",
                schema: "dbo",
                table: "OrganizationIntegrationConfigurations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Element14Enabled",
                schema: "dbo",
                table: "OrganizationIntegrationConfigurations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Element14PartNumber",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "Element14ApiKey",
                schema: "dbo",
                table: "OrganizationIntegrationConfigurations");

            migrationBuilder.DropColumn(
                name: "Element14ApiUrl",
                schema: "dbo",
                table: "OrganizationIntegrationConfigurations");

            migrationBuilder.DropColumn(
                name: "Element14Country",
                schema: "dbo",
                table: "OrganizationIntegrationConfigurations");

            migrationBuilder.DropColumn(
                name: "Element14Enabled",
                schema: "dbo",
                table: "OrganizationIntegrationConfigurations");
        }
    }
}
