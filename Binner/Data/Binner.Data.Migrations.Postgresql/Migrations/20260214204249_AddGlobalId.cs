using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Postgresql.Migrations
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
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "StoredFiles",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Projects",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "ProjectPcbAssignments",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "ProjectPartAssignments",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PcbStoredFileAssignments",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Pcbs",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartTypes",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartSuppliers",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Parts",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartParametrics",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "PartModels",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "Organizations",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "CustomFieldValues",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<Guid>(
                name: "GlobalId",
                schema: "dbo",
                table: "CustomFields",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");
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
