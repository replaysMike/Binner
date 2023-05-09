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

            migrationBuilder.Sql(@"
INSERT INTO LabelTemplates (Name, Width, Height, Margin, Dpi, LabelPaperSource, OrganizationId, UserId) VALUES('30277 (Dual 9/16"" x 3 7/16"")', '3.4375','0.5625', '0', '300', 0, 1, 1);
INSERT INTO LabelTemplates (Name, Width, Height, Margin, Dpi, LabelPaperSource, OrganizationId, UserId) VALUES('30346 (1/2"" x 1 7/8"")', '1.875','0.5', '0', '300', 0, 1, 1);");
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
