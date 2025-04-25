using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class AddTmeResolveExternalLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OAuthCredentials_Provider_UserId_OrganizationId",
                schema: "dbo",
                table: "OAuthCredentials",
                columns: new[] { "Provider", "UserId", "OrganizationId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OAuthCredentials_Provider_UserId_OrganizationId",
                schema: "dbo",
                table: "OAuthCredentials");
        }
    }
}
