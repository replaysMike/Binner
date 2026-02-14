using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.SqlServer.Migrations
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
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "StoredFiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "ProjectPcbAssignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "ProjectPartAssignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PcbStoredFileAssignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Pcbs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartSuppliers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Parts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartParametrics",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartModels",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Organizations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "CustomFieldValues",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "CustomFields",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");
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
