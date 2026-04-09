using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddPrintSpoolQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PrintSpoolQueueId",
                schema: "dbo",
                table: "OrganizationConfigurations",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrintSpoolQueueId",
                schema: "dbo",
                table: "OrganizationConfigurations");
        }
    }
}
