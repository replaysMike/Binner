using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddPartScanHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartScanHistories",
                schema: "dbo",
                columns: table => new
                {
                    PartScanHistoryId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartId = table.Column<long>(type: "INTEGER", nullable: true),
                    RawScan = table.Column<string>(type: "TEXT", nullable: false),
                    Crc = table.Column<int>(type: "INTEGER", nullable: false),
                    BarcodeType = table.Column<int>(type: "INTEGER", nullable: false),
                    ScannedLabelType = table.Column<int>(type: "INTEGER", nullable: false),
                    Supplier = table.Column<int>(type: "INTEGER", nullable: false),
                    ManufacturerPartNumber = table.Column<string>(type: "TEXT", nullable: true),
                    SupplierPartNumber = table.Column<string>(type: "TEXT", nullable: true),
                    SalesOrder = table.Column<string>(type: "TEXT", nullable: true),
                    Invoice = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Mid = table.Column<string>(type: "TEXT", nullable: true),
                    LotCode = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CountryOfOrigin = table.Column<string>(type: "TEXT", nullable: true),
                    Packlist = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartScanHistories", x => x.PartScanHistoryId);
                    table.ForeignKey(
                        name: "FK_PartScanHistories_Parts_PartId",
                        column: x => x.PartId,
                        principalSchema: "dbo",
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartScanHistories_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartScanHistories_PartId",
                schema: "dbo",
                table: "PartScanHistories",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_PartScanHistories_UserId",
                schema: "dbo",
                table: "PartScanHistories",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartScanHistories",
                schema: "dbo");
        }
    }
}
