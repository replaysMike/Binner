using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomPartFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtensionValue1",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtensionValue2",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FootprintName",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SymbolName",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtensionValue1",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ExtensionValue2",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "FootprintName",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "SymbolName",
                schema: "dbo",
                table: "Parts");
        }
    }
}
