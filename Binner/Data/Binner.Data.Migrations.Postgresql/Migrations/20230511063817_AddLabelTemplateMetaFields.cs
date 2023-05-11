using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Postgresql.Migrations
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
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DriverName",
                schema: "dbo",
                table: "LabelTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DriverWidth",
                schema: "dbo",
                table: "LabelTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExtraData",
                schema: "dbo",
                table: "LabelTemplates",
                type: "text",
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
