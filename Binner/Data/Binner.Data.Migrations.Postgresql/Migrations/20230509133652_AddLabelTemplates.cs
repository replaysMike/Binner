using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Binner.Data.Migrations.Postgresql.Migrations
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
                    LabelTemplateId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Width = table.Column<string>(type: "text", nullable: false),
                    Height = table.Column<string>(type: "text", nullable: false),
                    Margin = table.Column<string>(type: "text", nullable: false),
                    Dpi = table.Column<int>(type: "integer", nullable: false),
                    LabelPaperSource = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()")
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
                    LabelId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LabelTemplateId = table.Column<int>(type: "integer", nullable: false),
                    Template = table.Column<string>(type: "text", nullable: false),
                    IsPartLabelTemplate = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()")
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
INSERT INTO dbo.""LabelTemplates"" (""Name"", ""Width"", ""Height"", ""Margin"", ""Dpi"", ""LabelPaperSource"", ""OrganizationId"", ""UserId"") VALUES('30277', '3.4375','0.5625', '0', '300', 0, 1, 1);
INSERT INTO dbo.""LabelTemplates"" (""Name"", ""Width"", ""Height"", ""Margin"", ""Dpi"", ""LabelPaperSource"", ""OrganizationId"", ""UserId"") VALUES('30346', '1.875','0.5', '0', '300', 0, 1, 1);");
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
