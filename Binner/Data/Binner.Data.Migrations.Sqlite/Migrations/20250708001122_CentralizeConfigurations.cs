using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class CentralizeConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBarcodeConfigurations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserGlobalConfigurations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserIntegrationConfigurations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserLocaleConfigurations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserPreferences",
                schema: "dbo");

            migrationBuilder.CreateTable(
                name: "OrganizationConfigurations",
                schema: "dbo",
                columns: table => new
                {
                    OrganizationConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true),
                    LicenseKey = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationConfigurations", x => x.OrganizationConfigurationId);
                    table.ForeignKey(
                        name: "FK_OrganizationConfigurations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "dbo",
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationConfigurations_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "OrganizationIntegrationConfigurations",
                schema: "dbo",
                columns: table => new
                {
                    OrganizationIntegrationConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true),
                    SwarmEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    SwarmApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    SwarmApiUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SwarmTimeout = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    DigiKeyEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    DigiKeySite = table.Column<int>(type: "INTEGER", nullable: false),
                    DigiKeyClientId = table.Column<string>(type: "TEXT", nullable: true),
                    DigiKeyClientSecret = table.Column<string>(type: "TEXT", nullable: true),
                    DigiKeyOAuthPostbackUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DigiKeyApiUrl = table.Column<string>(type: "TEXT", nullable: true),
                    MouserEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    MouserSearchApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    MouserOrderApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    MouserCartApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    MouserApiUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ArrowEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArrowUsername = table.Column<string>(type: "TEXT", nullable: true),
                    ArrowApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    ArrowApiUrl = table.Column<string>(type: "TEXT", nullable: false),
                    NexarEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    NexarClientId = table.Column<string>(type: "TEXT", nullable: true),
                    NexarClientSecret = table.Column<string>(type: "TEXT", nullable: true),
                    TmeEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TmeCountry = table.Column<string>(type: "TEXT", nullable: true),
                    TmeApplicationSecret = table.Column<string>(type: "TEXT", nullable: true),
                    TmeApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    TmeApiUrl = table.Column<string>(type: "TEXT", nullable: false),
                    TmeResolveExternalLinks = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationIntegrationConfigurations", x => x.OrganizationIntegrationConfigurationId);
                    table.ForeignKey(
                        name: "FK_OrganizationIntegrationConfigurations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "dbo",
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationIntegrationConfigurations_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserConfigurations",
                schema: "dbo",
                columns: table => new
                {
                    UserConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true),
                    Language = table.Column<int>(type: "INTEGER", nullable: false),
                    Currency = table.Column<int>(type: "INTEGER", nullable: false),
                    EnableAutoPartSearch = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableDarkMode = table.Column<bool>(type: "INTEGER", nullable: false),
                    BarcodeEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    BarcodeIsDebug = table.Column<bool>(type: "INTEGER", nullable: false),
                    BarcodeMaxKeystrokeThresholdMs = table.Column<int>(type: "INTEGER", nullable: false),
                    BarcodeBufferTime = table.Column<int>(type: "INTEGER", nullable: false),
                    BarcodePrefix2D = table.Column<string>(type: "TEXT", nullable: false),
                    BarcodeProfile = table.Column<int>(type: "INTEGER", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConfigurations", x => x.UserConfigurationId);
                    table.ForeignKey(
                        name: "FK_UserConfigurations_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationConfigurations_OrganizationId",
                schema: "dbo",
                table: "OrganizationConfigurations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationConfigurations_UserId",
                schema: "dbo",
                table: "OrganizationConfigurations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationIntegrationConfigurations_OrganizationId",
                schema: "dbo",
                table: "OrganizationIntegrationConfigurations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationIntegrationConfigurations_UserId",
                schema: "dbo",
                table: "OrganizationIntegrationConfigurations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConfigurations_UserId",
                schema: "dbo",
                table: "UserConfigurations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationConfigurations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "OrganizationIntegrationConfigurations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserConfigurations",
                schema: "dbo");

            migrationBuilder.CreateTable(
                name: "UserBarcodeConfigurations",
                schema: "dbo",
                columns: table => new
                {
                    UserBarcodeConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    BufferTime = table.Column<int>(type: "INTEGER", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDebug = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxKeystrokeThresholdMs = table.Column<int>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true),
                    Prefix2D = table.Column<string>(type: "TEXT", nullable: false),
                    Profile = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBarcodeConfigurations", x => x.UserBarcodeConfigurationId);
                    table.ForeignKey(
                        name: "FK_UserBarcodeConfigurations_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserGlobalConfigurations",
                schema: "dbo",
                columns: table => new
                {
                    UserGlobalConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    LicenseKey = table.Column<string>(type: "TEXT", nullable: true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true)
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
                name: "UserIntegrationConfigurations",
                schema: "dbo",
                columns: table => new
                {
                    UserIntegrationConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    ArrowApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    ArrowApiUrl = table.Column<string>(type: "TEXT", nullable: false),
                    ArrowEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArrowUsername = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DigiKeyApiUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DigiKeyClientId = table.Column<string>(type: "TEXT", nullable: true),
                    DigiKeyClientSecret = table.Column<string>(type: "TEXT", nullable: true),
                    DigiKeyEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    DigiKeyOAuthPostbackUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DigiKeySite = table.Column<int>(type: "INTEGER", nullable: false),
                    MouserApiUrl = table.Column<string>(type: "TEXT", nullable: true),
                    MouserCartApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    MouserEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    MouserOrderApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    MouserSearchApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    OctopartClientId = table.Column<string>(type: "TEXT", nullable: true),
                    OctopartClientSecret = table.Column<string>(type: "TEXT", nullable: true),
                    OctopartEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true),
                    SwarmApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    SwarmApiUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SwarmEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    SwarmTimeout = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    TmeApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    TmeApiUrl = table.Column<string>(type: "TEXT", nullable: false),
                    TmeApplicationSecret = table.Column<string>(type: "TEXT", nullable: true),
                    TmeCountry = table.Column<string>(type: "TEXT", nullable: true),
                    TmeEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TmeResolveExternalLinks = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIntegrationConfigurations", x => x.UserIntegrationConfigurationId);
                    table.ForeignKey(
                        name: "FK_UserIntegrationConfigurations_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserLocaleConfigurations",
                schema: "dbo",
                columns: table => new
                {
                    UserLocaleConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Currency = table.Column<int>(type: "INTEGER", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    Language = table.Column<int>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLocaleConfigurations", x => x.UserLocaleConfigurationId);
                    table.ForeignKey(
                        name: "FK_UserLocaleConfigurations_Users_UserId",
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
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    EnableAutoPartSearch = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableDarkMode = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true)
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
                name: "IX_UserBarcodeConfigurations_UserId",
                schema: "dbo",
                table: "UserBarcodeConfigurations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGlobalConfigurations_UserId",
                schema: "dbo",
                table: "UserGlobalConfigurations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserIntegrationConfigurations_UserId",
                schema: "dbo",
                table: "UserIntegrationConfigurations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLocaleConfigurations_UserId",
                schema: "dbo",
                table: "UserLocaleConfigurations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                schema: "dbo",
                table: "UserPreferences",
                column: "UserId");
        }
    }
}
