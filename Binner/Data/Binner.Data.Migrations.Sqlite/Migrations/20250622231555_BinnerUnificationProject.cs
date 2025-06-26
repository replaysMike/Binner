using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class BinnerUnificationProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrintMode",
                schema: "dbo",
                table: "UserPrinterConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DigiKeySite",
                schema: "dbo",
                table: "UserIntegrationConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SwarmPartNumberManufacturerId",
                schema: "dbo",
                table: "Parts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateModifiedUtc",
                schema: "dbo",
                table: "Organizations",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "getutcdate()",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateCreatedUtc",
                schema: "dbo",
                table: "Organizations",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "getutcdate()",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                schema: "dbo",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                schema: "dbo",
                table: "Users",
                column: "OrganizationId",
                principalSchema: "dbo",
                principalTable: "Organizations",
                principalColumn: "OrganizationId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PrintMode",
                schema: "dbo",
                table: "UserPrinterConfigurations");

            migrationBuilder.DropColumn(
                name: "DigiKeySite",
                schema: "dbo",
                table: "UserIntegrationConfigurations");

            migrationBuilder.DropColumn(
                name: "SwarmPartNumberManufacturerId",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateModifiedUtc",
                schema: "dbo",
                table: "Organizations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "getutcdate()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateCreatedUtc",
                schema: "dbo",
                table: "Organizations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "getutcdate()");
        }
    }
}
