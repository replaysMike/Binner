using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class UnrestrictUserIdDeletions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_Users_UserId",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropForeignKey(
                name: "FK_PartSuppliers_Users_UserId",
                schema: "dbo",
                table: "PartSuppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_PartTypes_Users_UserId",
                schema: "dbo",
                table: "PartTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Pcbs_Users_UserId",
                schema: "dbo",
                table: "Pcbs");

            migrationBuilder.DropForeignKey(
                name: "FK_PcbStoredFileAssignments_Users_UserId",
                schema: "dbo",
                table: "PcbStoredFileAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectPartAssignments_Users_UserId",
                schema: "dbo",
                table: "ProjectPartAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectPcbAssignments_Users_UserId",
                schema: "dbo",
                table: "ProjectPcbAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectPcbProduceHistory_Users_UserId",
                schema: "dbo",
                table: "ProjectPcbProduceHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectProduceHistory_Users_UserId",
                schema: "dbo",
                table: "ProjectProduceHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_UserId",
                schema: "dbo",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_StoredFiles_Users_UserId",
                schema: "dbo",
                table: "StoredFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserIntegrationConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserIntegrationConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLoginHistory_Users_UserId",
                schema: "dbo",
                table: "UserLoginHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrinterConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserPrinterConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrinterTemplateConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserPrinterTemplateConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_Users_UserId",
                schema: "dbo",
                table: "UserTokens");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_Users_UserId",
                schema: "dbo",
                table: "Parts",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PartSuppliers_Users_UserId",
                schema: "dbo",
                table: "PartSuppliers",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PartTypes_Users_UserId",
                schema: "dbo",
                table: "PartTypes",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Pcbs_Users_UserId",
                schema: "dbo",
                table: "Pcbs",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PcbStoredFileAssignments_Users_UserId",
                schema: "dbo",
                table: "PcbStoredFileAssignments",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectPartAssignments_Users_UserId",
                schema: "dbo",
                table: "ProjectPartAssignments",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectPcbAssignments_Users_UserId",
                schema: "dbo",
                table: "ProjectPcbAssignments",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectPcbProduceHistory_Users_UserId",
                schema: "dbo",
                table: "ProjectPcbProduceHistory",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectProduceHistory_Users_UserId",
                schema: "dbo",
                table: "ProjectProduceHistory",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_UserId",
                schema: "dbo",
                table: "Projects",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StoredFiles_Users_UserId",
                schema: "dbo",
                table: "StoredFiles",
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
                name: "FK_UserLoginHistory_Users_UserId",
                schema: "dbo",
                table: "UserLoginHistory",
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
                name: "FK_UserPrinterTemplateConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserPrinterTemplateConfigurations",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_Users_UserId",
                schema: "dbo",
                table: "UserTokens",
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
                name: "FK_Parts_Users_UserId",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropForeignKey(
                name: "FK_PartSuppliers_Users_UserId",
                schema: "dbo",
                table: "PartSuppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_PartTypes_Users_UserId",
                schema: "dbo",
                table: "PartTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Pcbs_Users_UserId",
                schema: "dbo",
                table: "Pcbs");

            migrationBuilder.DropForeignKey(
                name: "FK_PcbStoredFileAssignments_Users_UserId",
                schema: "dbo",
                table: "PcbStoredFileAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectPartAssignments_Users_UserId",
                schema: "dbo",
                table: "ProjectPartAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectPcbAssignments_Users_UserId",
                schema: "dbo",
                table: "ProjectPcbAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectPcbProduceHistory_Users_UserId",
                schema: "dbo",
                table: "ProjectPcbProduceHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectProduceHistory_Users_UserId",
                schema: "dbo",
                table: "ProjectProduceHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_UserId",
                schema: "dbo",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_StoredFiles_Users_UserId",
                schema: "dbo",
                table: "StoredFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserIntegrationConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserIntegrationConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLoginHistory_Users_UserId",
                schema: "dbo",
                table: "UserLoginHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrinterConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserPrinterConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrinterTemplateConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserPrinterTemplateConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_Users_UserId",
                schema: "dbo",
                table: "UserTokens");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_Users_UserId",
                schema: "dbo",
                table: "Parts",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartSuppliers_Users_UserId",
                schema: "dbo",
                table: "PartSuppliers",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartTypes_Users_UserId",
                schema: "dbo",
                table: "PartTypes",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pcbs_Users_UserId",
                schema: "dbo",
                table: "Pcbs",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PcbStoredFileAssignments_Users_UserId",
                schema: "dbo",
                table: "PcbStoredFileAssignments",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectPartAssignments_Users_UserId",
                schema: "dbo",
                table: "ProjectPartAssignments",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectPcbAssignments_Users_UserId",
                schema: "dbo",
                table: "ProjectPcbAssignments",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectPcbProduceHistory_Users_UserId",
                schema: "dbo",
                table: "ProjectPcbProduceHistory",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectProduceHistory_Users_UserId",
                schema: "dbo",
                table: "ProjectProduceHistory",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_UserId",
                schema: "dbo",
                table: "Projects",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StoredFiles_Users_UserId",
                schema: "dbo",
                table: "StoredFiles",
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
                name: "FK_UserLoginHistory_Users_UserId",
                schema: "dbo",
                table: "UserLoginHistory",
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
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrinterTemplateConfigurations_Users_UserId",
                schema: "dbo",
                table: "UserPrinterTemplateConfigurations",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_Users_UserId",
                schema: "dbo",
                table: "UserTokens",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
