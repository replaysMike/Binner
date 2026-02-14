using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Sqlite.Migrations
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
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "StoredFiles",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "ProjectPcbAssignments",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "ProjectPartAssignments",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PcbStoredFileAssignments",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Pcbs",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartTypes",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartSuppliers",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Parts",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartParametrics",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartModels",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Organizations",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "CustomFieldValues",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "CustomFields",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
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
