using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddLabelTemplateMetaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DriverHeight",
                schema: "dbo",
                table: "LabelTemplates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DriverName",
                schema: "dbo",
                table: "LabelTemplates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DriverWidth",
                schema: "dbo",
                table: "LabelTemplates",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExtraData",
                schema: "dbo",
                table: "LabelTemplates",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverHeight",
                schema: "dbo",
                table: "LabelTemplates");

            migrationBuilder.DropColumn(
                name: "DriverName",
                schema: "dbo",
                table: "LabelTemplates");

            migrationBuilder.DropColumn(
                name: "DriverWidth",
                schema: "dbo",
                table: "LabelTemplates");

            migrationBuilder.DropColumn(
                name: "ExtraData",
                schema: "dbo",
                table: "LabelTemplates");
        }
    }
}
