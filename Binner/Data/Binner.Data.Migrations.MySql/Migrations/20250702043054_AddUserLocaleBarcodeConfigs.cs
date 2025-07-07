using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLocaleBarcodeConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserBarcodeConfiguration",
                schema: "dbo",
                columns: table => new
                {
                    UserBarcodeConfigurationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDebug = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaxKeystrokeThresholdMs = table.Column<int>(type: "int", nullable: false),
                    BufferTime = table.Column<int>(type: "int", nullable: false),
                    Prefix2D = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Profile = table.Column<int>(type: "int", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBarcodeConfiguration", x => x.UserBarcodeConfigurationId);
                    table.ForeignKey(
                        name: "FK_UserBarcodeConfiguration_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserLocaleConfigurations",
                schema: "dbo",
                columns: table => new
                {
                    UserLocaleConfigurationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    Language = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "getutcdate()")
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
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserBarcodeConfiguration_UserId",
                schema: "dbo",
                table: "UserBarcodeConfiguration",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLocaleConfigurations_UserId",
                schema: "dbo",
                table: "UserLocaleConfigurations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBarcodeConfiguration",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserLocaleConfigurations",
                schema: "dbo");
        }
    }
}
