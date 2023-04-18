using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgsAndProduceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "UserTokens",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocaleCurrency",
                schema: "dbo",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocaleLanguage",
                schema: "dbo",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "UserPrinterTemplateConfigurations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "UserPrinterConfigurations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "UserLoginHistory",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "UserIntegrationConfigurations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "StoredFiles",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "Projects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "ProjectPcbAssignments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "ProjectPartAssignments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "PcbStoredFileAssignments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "Pcbs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "PartTypes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "PartSuppliers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "Parts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "OAuthRequests",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "dbo",
                table: "OAuthCredentials",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organizations",
                schema: "dbo",
                columns: table => new
                {
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.OrganizationId);
                });

            migrationBuilder.CreateTable(
                name: "ProjectProduceHistory",
                schema: "dbo",
                columns: table => new
                {
                    ProjectProduceHistoryId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<long>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    ProduceUnassociated = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectProduceHistory", x => x.ProjectProduceHistoryId);
                    table.ForeignKey(
                        name: "FK_ProjectProduceHistory_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "dbo",
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectProduceHistory_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPcbProduceHistory",
                schema: "dbo",
                columns: table => new
                {
                    ProjectPcbProduceHistoryId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectProduceHistoryId = table.Column<long>(type: "INTEGER", nullable: false),
                    PcbId = table.Column<long>(type: "INTEGER", nullable: true),
                    PcbQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    PcbCost = table.Column<double>(type: "REAL", nullable: false),
                    SerialNumber = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPcbProduceHistory", x => x.ProjectPcbProduceHistoryId);
                    table.ForeignKey(
                        name: "FK_ProjectPcbProduceHistory_Pcbs_PcbId",
                        column: x => x.PcbId,
                        principalSchema: "dbo",
                        principalTable: "Pcbs",
                        principalColumn: "PcbId");
                    table.ForeignKey(
                        name: "FK_ProjectPcbProduceHistory_ProjectProduceHistory_ProjectProduceHistoryId",
                        column: x => x.ProjectProduceHistoryId,
                        principalSchema: "dbo",
                        principalTable: "ProjectProduceHistory",
                        principalColumn: "ProjectProduceHistoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectPcbProduceHistory_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPcbProduceHistory_PcbId",
                schema: "dbo",
                table: "ProjectPcbProduceHistory",
                column: "PcbId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPcbProduceHistory_ProjectProduceHistoryId",
                schema: "dbo",
                table: "ProjectPcbProduceHistory",
                column: "ProjectProduceHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPcbProduceHistory_UserId",
                schema: "dbo",
                table: "ProjectPcbProduceHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectProduceHistory_ProjectId",
                schema: "dbo",
                table: "ProjectProduceHistory",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectProduceHistory_UserId",
                schema: "dbo",
                table: "ProjectProduceHistory",
                column: "UserId");

            migrationBuilder.Sql("INSERT INTO Organizations (Name, Description, DateCreatedUtc, DateModifiedUtc) VALUES('Default', 'Default Organization', DATETIME('now'), DATETIME('now'));");
            migrationBuilder.Sql("UPDATE OAuthCredentials SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE OAuthRequests SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE Parts SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE PartSuppliers SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE PartTypes SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE Pcbs SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE PcbStoredFileAssignments SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE ProjectPartAssignments SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE ProjectPcbAssignments SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE Projects SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE StoredFiles SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE UserIntegrationConfigurations SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE UserLoginHistory SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE UserPrinterConfigurations SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE UserPrinterTemplateConfigurations SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE Users SET OrganizationId = 1");
            migrationBuilder.Sql("UPDATE UserTokens SET OrganizationId = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Organizations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ProjectPcbProduceHistory",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ProjectProduceHistory",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "UserTokens");

            migrationBuilder.DropColumn(
                name: "LocaleCurrency",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LocaleLanguage",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "UserPrinterTemplateConfigurations");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "UserPrinterConfigurations");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "UserLoginHistory");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "UserIntegrationConfigurations");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "ProjectPcbAssignments");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "ProjectPartAssignments");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "PcbStoredFileAssignments");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "Pcbs");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "PartTypes");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "PartSuppliers");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "OAuthRequests");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "dbo",
                table: "OAuthCredentials");
        }
    }
}
