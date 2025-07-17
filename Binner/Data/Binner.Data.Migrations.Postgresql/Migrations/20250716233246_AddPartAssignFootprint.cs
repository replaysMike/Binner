using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class AddPartAssignFootprint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FootprintName",
                schema: "dbo",
                table: "ProjectPartAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SymbolName",
                schema: "dbo",
                table: "ProjectPartAssignments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FootprintName",
                schema: "dbo",
                table: "ProjectPartAssignments");

            migrationBuilder.DropColumn(
                name: "SymbolName",
                schema: "dbo",
                table: "ProjectPartAssignments");
        }
    }
}
