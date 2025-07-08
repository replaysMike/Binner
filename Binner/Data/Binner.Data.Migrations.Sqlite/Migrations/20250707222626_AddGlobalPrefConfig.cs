using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalPrefConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBarcodeConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserBarcodeConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserIntegrationConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserIntegrationConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLocaleConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserLocaleConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrinterConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserPrinterConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrinterTemplateConfigurations_UserPrinterConfigurations_UserPrinterConfigurationId",
                schema: "dbo",
                table: "UserPrinterTemplateConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrinterTemplateConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserPrinterTemplateConfigurations");

            migrationBuilder.CreateTable(
                name: "UserGlobalConfigurations",
                schema: "dbo",
                columns: table => new
                {
                    UserGlobalConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true),
                    LicenseKey = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGlobalConfigurations", x => x.UserGlobalConfigurationId);
                    table.ForeignKey(
                        name: "FK_UserGlobalConfigurations_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                schema: "dbo",
                columns: table => new
                {
                    UserPreferencesId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true),
                    EnableAutoPartSearch = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableDarkMode = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.UserPreferencesId);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGlobalConfigurations_UserId",
                schema: "dbo",
                table: "UserGlobalConfigurations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                schema: "dbo",
                table: "UserPreferences",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBarcodeConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserBarcodeConfigurations",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserIntegrationConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserIntegrationConfigurations",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLocaleConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserLocaleConfigurations",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrinterConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserPrinterConfigurations",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrinterTemplateConfigurations_UserPrinterConfigurations_UserPrinterConfigurationId",
                schema: "dbo",
                table: "UserPrinterTemplateConfigurations",
                column: "UserPrinterConfigurationId",
                principalSchema: "dbo",
                principalTable: "UserPrinterConfigurations",
                principalColumn: "UserPrinterConfigurationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrinterTemplateConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserPrinterTemplateConfigurations",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBarcodeConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserBarcodeConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserIntegrationConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserIntegrationConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLocaleConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserLocaleConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrinterConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserPrinterConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrinterTemplateConfigurations_UserPrinterConfigurations_UserPrinterConfigurationId",
                schema: "dbo",
                table: "UserPrinterTemplateConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrinterTemplateConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserPrinterTemplateConfigurations");

            migrationBuilder.DropTable(
                name: "UserGlobalConfigurations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserPreferences",
                schema: "dbo");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBarcodeConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserBarcodeConfigurations",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserIntegrationConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserIntegrationConfigurations",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLocaleConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserLocaleConfigurations",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrinterConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserPrinterConfigurations",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrinterTemplateConfigurations_UserPrinterConfigurations_UserPrinterConfigurationId",
                schema: "dbo",
                table: "UserPrinterTemplateConfigurations",
                column: "UserPrinterConfigurationId",
                principalSchema: "dbo",
                principalTable: "UserPrinterConfigurations",
                principalColumn: "UserPrinterConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrinterTemplateConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserPrinterTemplateConfigurations",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
