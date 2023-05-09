using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddLabelTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabelTemplates",
                schema: "dbo",
                columns: table => new
                {
                    LabelTemplateId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Width = table.Column<string>(type: "TEXT", nullable: false),
                    Height = table.Column<string>(type: "TEXT", nullable: false),
                    Margin = table.Column<string>(type: "TEXT", nullable: false),
                    Dpi = table.Column<int>(type: "INTEGER", nullable: false),
                    LabelPaperSource = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelTemplates", x => x.LabelTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "Labels",
                schema: "dbo",
                columns: table => new
                {
                    LabelId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LabelTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    Template = table.Column<string>(type: "TEXT", nullable: false),
                    IsPartLabelTemplate = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Labels", x => x.LabelId);
                    table.ForeignKey(
                        name: "FK_Labels_LabelTemplates_LabelTemplateId",
                        column: x => x.LabelTemplateId,
                        principalSchema: "dbo",
                        principalTable: "LabelTemplates",
                        principalColumn: "LabelTemplateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Labels_LabelTemplateId",
                schema: "dbo",
                table: "Labels",
                column: "LabelTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Labels",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LabelTemplates",
                schema: "dbo");
        }
    }
}
