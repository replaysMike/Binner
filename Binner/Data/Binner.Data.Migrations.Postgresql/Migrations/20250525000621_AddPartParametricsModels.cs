using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Binner.Data.Migrations.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class AddPartParametricsModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaseProductNumber",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DataSource",
                schema: "dbo",
                table: "Parts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DataSourceId",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExportControlClassNumber",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HtsusCode",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSwarmSyncUtc",
                schema: "dbo",
                table: "Parts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeadTime",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MetadataLastUpdatedUtc",
                schema: "dbo",
                table: "Parts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "MoistureSensitivityLevel",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherNames",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductStatus",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReachStatus",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RohsStatus",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Series",
                schema: "dbo",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PartModels",
                schema: "dbo",
                columns: table => new
                {
                    PartModelId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Filename = table.Column<string>(type: "text", nullable: true),
                    ModelType = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartModels", x => x.PartModelId);
                    table.ForeignKey(
                        name: "FK_PartModels_Parts_PartId",
                        column: x => x.PartId,
                        principalSchema: "dbo",
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartModels_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PartParametrics",
                schema: "dbo",
                columns: table => new
                {
                    PartParametricId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    ValueNumber = table.Column<double>(type: "numeric(18,4)", nullable: false),
                    Units = table.Column<int>(type: "integer", nullable: false),
                    DigiKeyParameterId = table.Column<int>(type: "integer", nullable: false),
                    DigiKeyValueId = table.Column<int>(type: "integer", nullable: false),
                    DigiKeyParameterType = table.Column<string>(type: "text", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartParametrics", x => x.PartParametricId);
                    table.ForeignKey(
                        name: "FK_PartParametrics_Parts_PartId",
                        column: x => x.PartId,
                        principalSchema: "dbo",
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartParametrics_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartModels_PartId_OrganizationId",
                schema: "dbo",
                table: "PartModels",
                columns: new[] { "PartId", "OrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_PartModels_UserId",
                schema: "dbo",
                table: "PartModels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartParametrics_PartId_OrganizationId",
                schema: "dbo",
                table: "PartParametrics",
                columns: new[] { "PartId", "OrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_PartParametrics_UserId",
                schema: "dbo",
                table: "PartParametrics",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartModels",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PartParametrics",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "BaseProductNumber",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "DataSource",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "DataSourceId",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ExportControlClassNumber",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "HtsusCode",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "LastSwarmSyncUtc",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "LeadTime",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "MetadataLastUpdatedUtc",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "MoistureSensitivityLevel",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "OtherNames",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ProductStatus",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ReachStatus",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "RohsStatus",
                schema: "dbo",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "Series",
                schema: "dbo",
                table: "Parts");
        }
    }
}
