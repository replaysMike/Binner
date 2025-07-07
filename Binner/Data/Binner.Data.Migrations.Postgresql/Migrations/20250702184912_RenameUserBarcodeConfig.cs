using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserBarcodeConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBarcodeConfiguration_Users_UserId",
                schema: "dbo",
                table: "UserBarcodeConfiguration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserBarcodeConfiguration",
                schema: "dbo",
                table: "UserBarcodeConfiguration");

            migrationBuilder.RenameTable(
                name: "UserBarcodeConfiguration",
                schema: "dbo",
                newName: "UserBarcodeConfigurations",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_UserBarcodeConfiguration_UserId",
                schema: "dbo",
                table: "UserBarcodeConfigurations",
                newName: "IX_UserBarcodeConfigurations_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserBarcodeConfigurations",
                schema: "dbo",
                table: "UserBarcodeConfigurations",
                column: "UserBarcodeConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBarcodeConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserBarcodeConfigurations",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
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
