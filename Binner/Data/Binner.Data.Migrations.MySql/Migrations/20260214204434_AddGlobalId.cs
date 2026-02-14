using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Users",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "StoredFiles",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Projects",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "ProjectPcbAssignments",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "ProjectPartAssignments",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PcbStoredFileAssignments",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Pcbs",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartTypes",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartSuppliers",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Parts",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartParametrics",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartModels",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Organizations",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "CustomFieldValues",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "CustomFields",
                type: "char(36)",
                nullable: false,
                defaultValueSql: "UUID()",
                collation: "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "ProjectPcbAssignments");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "ProjectPartAssignments");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "PcbStoredFileAssignments");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "Pcbs");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "PartTypes");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "PartSuppliers");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "PartParametrics");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "PartModels");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "CustomFieldValues");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                schema: "dbo",
                table: "CustomFields");
        }
    }
}
