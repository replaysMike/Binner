using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "OAuthCredentials",
                schema: "dbo",
                columns: table => new
                {
                    Provider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    DateExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthCredentials", x => x.Provider);
                });

            migrationBuilder.CreateTable(
                name: "OAuthRequests",
                schema: "dbo",
                columns: table => new
                {
                    OAuthRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    AuthorizationReceived = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AuthorizationCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnToUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthRequests", x => x.OAuthRequestId);
                });

            migrationBuilder.CreateTable(
                name: "PartTypes",
                schema: "dbo",
                columns: table => new
                {
                    PartTypeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ParentPartTypeId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartTypes", x => x.PartTypeId);
                    table.ForeignKey(
                        name: "FK_PartTypes_PartTypes_ParentPartTypeId",
                        column: x => x.ParentPartTypeId,
                        principalSchema: "dbo",
                        principalTable: "PartTypes",
                        principalColumn: "PartTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pcbs",
                schema: "dbo",
                columns: table => new
                {
                    PcbId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SerialNumberFormat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<double>(type: "float", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pcbs", x => x.PcbId);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                schema: "dbo",
                columns: table => new
                {
                    ProjectId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Color = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectId);
                });

            migrationBuilder.CreateTable(
                name: "Parts",
                schema: "dbo",
                columns: table => new
                {
                    PartId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DigiKeyPartNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MouserPartNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ArrowPartNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PartTypeId = table.Column<long>(type: "bigint", nullable: false),
                    MountingTypeId = table.Column<int>(type: "int", nullable: false),
                    PackageType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LowestCostSupplier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LowestCostSupplierUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectId = table.Column<long>(type: "bigint", nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DatasheetUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BinNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BinNumber2 = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ManufacturerPartNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.PartId);
                    table.ForeignKey(
                        name: "FK_Parts_PartTypes_PartTypeId",
                        column: x => x.PartTypeId,
                        principalSchema: "dbo",
                        principalTable: "PartTypes",
                        principalColumn: "PartTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "dbo",
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPcbAssignments",
                schema: "dbo",
                columns: table => new
                {
                    ProjectPcbAssignmentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    PcbId = table.Column<long>(type: "bigint", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPcbAssignments", x => x.ProjectPcbAssignmentId);
                    table.ForeignKey(
                        name: "FK_ProjectPcbAssignments_Pcbs_PcbId",
                        column: x => x.PcbId,
                        principalSchema: "dbo",
                        principalTable: "Pcbs",
                        principalColumn: "PcbId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectPcbAssignments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "dbo",
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartSuppliers",
                schema: "dbo",
                columns: table => new
                {
                    PartSupplierId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupplierPartNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    QuantityAvailable = table.Column<int>(type: "int", nullable: false),
                    MinimumOrderQuantity = table.Column<int>(type: "int", nullable: false),
                    ProductUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartSuppliers", x => x.PartSupplierId);
                    table.ForeignKey(
                        name: "FK_PartSuppliers_Parts_PartId",
                        column: x => x.PartId,
                        principalSchema: "dbo",
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPartAssignments",
                schema: "dbo",
                columns: table => new
                {
                    ProjectPartAssignmentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    PartId = table.Column<long>(type: "bigint", nullable: true),
                    PcbId = table.Column<long>(type: "bigint", nullable: true),
                    PartName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    QuantityAvailable = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchematicReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cost = table.Column<double>(type: "float", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPartAssignments", x => x.ProjectPartAssignmentId);
                    table.ForeignKey(
                        name: "FK_ProjectPartAssignments_Parts_PartId",
                        column: x => x.PartId,
                        principalSchema: "dbo",
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectPartAssignments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "dbo",
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoredFiles",
                schema: "dbo",
                columns: table => new
                {
                    StoredFileId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoredFileType = table.Column<int>(type: "int", nullable: false),
                    PartId = table.Column<long>(type: "bigint", nullable: true),
                    FileLength = table.Column<int>(type: "int", nullable: false),
                    Crc32 = table.Column<int>(type: "int", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.StoredFileId);
                    table.ForeignKey(
                        name: "FK_StoredFiles_Parts_PartId",
                        column: x => x.PartId,
                        principalSchema: "dbo",
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PcbStoredFileAssignments",
                schema: "dbo",
                columns: table => new
                {
                    PcbStoredFileAssignmentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PcbId = table.Column<long>(type: "bigint", nullable: false),
                    StoredFileId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcbStoredFileAssignments", x => x.PcbStoredFileAssignmentId);
                    table.ForeignKey(
                        name: "FK_PcbStoredFileAssignments_Pcbs_PcbId",
                        column: x => x.PcbId,
                        principalSchema: "dbo",
                        principalTable: "Pcbs",
                        principalColumn: "PcbId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PcbStoredFileAssignments_StoredFiles_StoredFileId",
                        column: x => x.StoredFileId,
                        principalSchema: "dbo",
                        principalTable: "StoredFiles",
                        principalColumn: "StoredFileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_BinNumber_UserId",
                schema: "dbo",
                table: "Parts",
                columns: new[] { "BinNumber", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_BinNumber2_UserId",
                schema: "dbo",
                table: "Parts",
                columns: new[] { "BinNumber2", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Description_UserId",
                schema: "dbo",
                table: "Parts",
                columns: new[] { "Description", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_DigiKeyPartNumber_UserId",
                schema: "dbo",
                table: "Parts",
                columns: new[] { "DigiKeyPartNumber", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Keywords_UserId",
                schema: "dbo",
                table: "Parts",
                columns: new[] { "Keywords", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Location_UserId",
                schema: "dbo",
                table: "Parts",
                columns: new[] { "Location", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Manufacturer_UserId",
                schema: "dbo",
                table: "Parts",
                columns: new[] { "Manufacturer", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_ManufacturerPartNumber_UserId",
                schema: "dbo",
                table: "Parts",
                columns: new[] { "ManufacturerPartNumber", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_MouserPartNumber_UserId",
                schema: "dbo",
                table: "Parts",
                columns: new[] { "MouserPartNumber", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartNumber_UserId",
                schema: "dbo",
                table: "Parts",
                columns: new[] { "PartNumber", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartTypeId_UserId",
                schema: "dbo",
                table: "Parts",
                columns: new[] { "PartTypeId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_ProjectId",
                schema: "dbo",
                table: "Parts",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PartSuppliers_PartId",
                schema: "dbo",
                table: "PartSuppliers",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_PartTypes_Name_UserId",
                schema: "dbo",
                table: "PartTypes",
                columns: new[] { "Name", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_PartTypes_ParentPartTypeId",
                schema: "dbo",
                table: "PartTypes",
                column: "ParentPartTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PcbStoredFileAssignments_PcbId",
                schema: "dbo",
                table: "PcbStoredFileAssignments",
                column: "PcbId");

            migrationBuilder.CreateIndex(
                name: "IX_PcbStoredFileAssignments_StoredFileId",
                schema: "dbo",
                table: "PcbStoredFileAssignments",
                column: "StoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPartAssignments_PartId",
                schema: "dbo",
                table: "ProjectPartAssignments",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPartAssignments_ProjectId",
                schema: "dbo",
                table: "ProjectPartAssignments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPcbAssignments_PcbId",
                schema: "dbo",
                table: "ProjectPcbAssignments",
                column: "PcbId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPcbAssignments_ProjectId",
                schema: "dbo",
                table: "ProjectPcbAssignments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name_UserId",
                schema: "dbo",
                table: "Projects",
                columns: new[] { "Name", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_PartId",
                schema: "dbo",
                table: "StoredFiles",
                column: "PartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OAuthCredentials",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "OAuthRequests",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PartSuppliers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PcbStoredFileAssignments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ProjectPartAssignments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ProjectPcbAssignments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "StoredFiles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Pcbs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Parts",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PartTypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Projects",
                schema: "dbo");
        }
    }
}
