using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.MySql.Migrations
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
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ExtensionValue2",
                schema: "dbo",
                table: "Parts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FootprintName",
                schema: "dbo",
                table: "Parts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SymbolName",
                schema: "dbo",
                table: "Parts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
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
