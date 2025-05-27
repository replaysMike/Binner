using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddDKValueFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DigiKeyParameterText",
                schema: "dbo",
                table: "PartParametrics",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DigiKeyValueText",
                schema: "dbo",
                table: "PartParametrics",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DigiKeyParameterText",
                schema: "dbo",
                table: "PartParametrics");

            migrationBuilder.DropColumn(
                name: "DigiKeyValueText",
                schema: "dbo",
                table: "PartParametrics");
        }
    }
}
