using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Binner.Data.Migrations.Postgresql.Migrations
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
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

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
