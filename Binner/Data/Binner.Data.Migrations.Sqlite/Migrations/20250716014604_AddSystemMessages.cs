using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageStates",
                schema: "dbo",
                columns: table => new
                {
                    MessageStateId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReadDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageStates", x => x.MessageStateId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageStates",
                schema: "dbo");
        }
    }
}
