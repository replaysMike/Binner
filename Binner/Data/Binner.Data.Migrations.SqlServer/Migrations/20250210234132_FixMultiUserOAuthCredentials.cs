using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class FixMultiUserOAuthCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OAuthCredentials",
                schema: "dbo",
                table: "OAuthCredentials");

            migrationBuilder.AddColumn<int>(
                name: "OAuthCredentialId",
                schema: "dbo",
                table: "OAuthCredentials",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OAuthCredentials",
                schema: "dbo",
                table: "OAuthCredentials",
                column: "OAuthCredentialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OAuthCredentials",
                schema: "dbo",
                table: "OAuthCredentials");

            migrationBuilder.DropColumn(
                name: "OAuthCredentialId",
                schema: "dbo",
                table: "OAuthCredentials");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OAuthCredentials",
                schema: "dbo",
                table: "OAuthCredentials",
                column: "Provider");
        }
    }
}
