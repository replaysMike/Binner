using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredFileRecordType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RecordId",
                schema: "dbo",
                table: "StoredFiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecordType",
                schema: "dbo",
                table: "StoredFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordId",
                schema: "dbo",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "RecordType",
                schema: "dbo",
                table: "StoredFiles");
        }
    }
}
