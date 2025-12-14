using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class automaticMetadataFetch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "enableAutomaticMetadataFetchingForExistingParts",
                schema: "dbo",
                table: "OrganizationConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "enableAutomaticMetadataFetchingForExistingParts",
                schema: "dbo",
                table: "OrganizationConfigurations");
        }
    }
}
