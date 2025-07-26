using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Postgresql.Migrations
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
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Element14ApiKey",
                schema: "dbo",
                table: "OrganizationIntegrationConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Element14ApiUrl",
                schema: "dbo",
                table: "OrganizationIntegrationConfigurations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Element14Country",
                schema: "dbo",
                table: "OrganizationIntegrationConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Element14Enabled",
                schema: "dbo",
                table: "OrganizationIntegrationConfigurations",
                type: "boolean",
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
