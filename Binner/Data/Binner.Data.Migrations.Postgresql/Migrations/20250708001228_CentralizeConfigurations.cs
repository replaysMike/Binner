using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Binner.Data.Migrations.Postgresql.Migrations
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
                    OrganizationConfigurationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    LicenseKey = table.Column<string>(type: "text", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "integer", nullable: true)
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
                    OrganizationIntegrationConfigurationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    SwarmEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SwarmApiKey = table.Column<string>(type: "text", nullable: true),
                    SwarmApiUrl = table.Column<string>(type: "text", nullable: true),
                    SwarmTimeout = table.Column<TimeSpan>(type: "interval", nullable: true),
                    DigiKeyEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DigiKeySite = table.Column<int>(type: "integer", nullable: false),
                    DigiKeyClientId = table.Column<string>(type: "text", nullable: true),
                    DigiKeyClientSecret = table.Column<string>(type: "text", nullable: true),
                    DigiKeyOAuthPostbackUrl = table.Column<string>(type: "text", nullable: true),
                    DigiKeyApiUrl = table.Column<string>(type: "text", nullable: true),
                    MouserEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MouserSearchApiKey = table.Column<string>(type: "text", nullable: true),
                    MouserOrderApiKey = table.Column<string>(type: "text", nullable: true),
                    MouserCartApiKey = table.Column<string>(type: "text", nullable: true),
                    MouserApiUrl = table.Column<string>(type: "text", nullable: true),
                    ArrowEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ArrowUsername = table.Column<string>(type: "text", nullable: true),
                    ArrowApiKey = table.Column<string>(type: "text", nullable: true),
                    ArrowApiUrl = table.Column<string>(type: "text", nullable: false),
                    NexarEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    NexarClientId = table.Column<string>(type: "text", nullable: true),
                    NexarClientSecret = table.Column<string>(type: "text", nullable: true),
                    TmeEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TmeCountry = table.Column<string>(type: "text", nullable: true),
                    TmeApplicationSecret = table.Column<string>(type: "text", nullable: true),
                    TmeApiKey = table.Column<string>(type: "text", nullable: true),
                    TmeApiUrl = table.Column<string>(type: "text", nullable: false),
                    TmeResolveExternalLinks = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationIntegrationConfigurations", x => x.OrganizationIntegrationConfigurationId);
                    table.ForeignKey(
                        name: "FK_OrganizationIntegrationConfigurations_Organizations_Organiz~",
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
                    UserConfigurationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    Language = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    EnableAutoPartSearch = table.Column<bool>(type: "boolean", nullable: false),
                    EnableDarkMode = table.Column<bool>(type: "boolean", nullable: false),
                    BarcodeEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    BarcodeIsDebug = table.Column<bool>(type: "boolean", nullable: false),
                    BarcodeMaxKeystrokeThresholdMs = table.Column<int>(type: "integer", nullable: false),
                    BarcodeBufferTime = table.Column<int>(type: "integer", nullable: false),
                    BarcodePrefix2D = table.Column<string>(type: "text", nullable: false),
                    BarcodeProfile = table.Column<int>(type: "integer", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()")
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
                    UserBarcodeConfigurationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    BufferTime = table.Column<int>(type: "integer", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsDebug = table.Column<bool>(type: "boolean", nullable: false),
                    MaxKeystrokeThresholdMs = table.Column<int>(type: "integer", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    Prefix2D = table.Column<string>(type: "text", nullable: false),
                    Profile = table.Column<int>(type: "integer", nullable: false)
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
                    UserGlobalConfigurationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    LicenseKey = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true)
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
                    UserIntegrationConfigurationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    ArrowApiKey = table.Column<string>(type: "text", nullable: true),
                    ArrowApiUrl = table.Column<string>(type: "text", nullable: false),
                    ArrowEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ArrowUsername = table.Column<string>(type: "text", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    DigiKeyApiUrl = table.Column<string>(type: "text", nullable: true),
                    DigiKeyClientId = table.Column<string>(type: "text", nullable: true),
                    DigiKeyClientSecret = table.Column<string>(type: "text", nullable: true),
                    DigiKeyEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DigiKeyOAuthPostbackUrl = table.Column<string>(type: "text", nullable: true),
                    DigiKeySite = table.Column<int>(type: "integer", nullable: false),
                    MouserApiUrl = table.Column<string>(type: "text", nullable: true),
                    MouserCartApiKey = table.Column<string>(type: "text", nullable: true),
                    MouserEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MouserOrderApiKey = table.Column<string>(type: "text", nullable: true),
                    MouserSearchApiKey = table.Column<string>(type: "text", nullable: true),
                    OctopartClientId = table.Column<string>(type: "text", nullable: true),
                    OctopartClientSecret = table.Column<string>(type: "text", nullable: true),
                    OctopartEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    SwarmApiKey = table.Column<string>(type: "text", nullable: true),
                    SwarmApiUrl = table.Column<string>(type: "text", nullable: true),
                    SwarmEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SwarmTimeout = table.Column<TimeSpan>(type: "interval", nullable: true),
                    TmeApiKey = table.Column<string>(type: "text", nullable: true),
                    TmeApiUrl = table.Column<string>(type: "text", nullable: false),
                    TmeApplicationSecret = table.Column<string>(type: "text", nullable: true),
                    TmeCountry = table.Column<string>(type: "text", nullable: true),
                    TmeEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TmeResolveExternalLinks = table.Column<bool>(type: "boolean", nullable: false)
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
                    UserLocaleConfigurationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    Language = table.Column<int>(type: "integer", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true)
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
                    UserPreferencesId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    EnableAutoPartSearch = table.Column<bool>(type: "boolean", nullable: false),
                    EnableDarkMode = table.Column<bool>(type: "boolean", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true)
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
