using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddTmePartNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TmePartNumber",
                schema: "dbo",
                table: "Parts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TmePartNumber",
                schema: "dbo",
                table: "Parts");
        }
    }
}
