using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserBarcodeConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* MySQL migration generator created a migration that doesn't work. Alternatively dropping the table as there is no data as of yet */

            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.userbarcodeconfiguration");

            migrationBuilder.CreateTable(
                 name: "UserBarcodeConfigurations",
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
                     table.PrimaryKey("PK_UserBarcodeConfigurations", x => x.UserBarcodeConfigurationId);
                     table.ForeignKey(
                         name: "FK_UserBarcodeConfigurations_Users_UserId",
                         column: x => x.UserId,
                         principalSchema: "dbo",
                         principalTable: "Users",
                         principalColumn: "UserId",
                         onDelete: ReferentialAction.SetNull);
                 })
                 .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserBarcodeConfigurations_UserId",
                schema: "dbo",
                table: "UserBarcodeConfigurations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBarcodeConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserBarcodeConfigurations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserBarcodeConfigurations",
                schema: "dbo",
                table: "UserBarcodeConfigurations");

            migrationBuilder.RenameTable(
                name: "UserBarcodeConfigurations",
                schema: "dbo",
                newName: "UserBarcodeConfiguration",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_UserBarcodeConfigurations_UserId",
                schema: "dbo",
                table: "UserBarcodeConfiguration",
                newName: "IX_UserBarcodeConfiguration_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserBarcodeConfiguration",
                schema: "dbo",
                table: "UserBarcodeConfiguration",
                column: "UserBarcodeConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBarcodeConfiguration_Users_UserId",
                schema: "dbo",
                table: "UserBarcodeConfiguration",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
