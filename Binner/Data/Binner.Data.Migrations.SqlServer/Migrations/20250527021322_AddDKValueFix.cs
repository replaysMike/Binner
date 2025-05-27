using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.SqlServer.Migrations
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
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DigiKeyValueText",
                schema: "dbo",
                table: "PartParametrics",
                type: "nvarchar(max)",
                nullable: true);
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
