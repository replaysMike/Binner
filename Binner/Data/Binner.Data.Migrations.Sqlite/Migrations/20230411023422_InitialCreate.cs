using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Binner.Data.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pcbs",
                columns: table => new
                {
                    PcbId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    SerialNumberFormat = table.Column<string>(type: "TEXT", nullable: true),
                    LastSerialNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Cost = table.Column<double>(type: "REAL", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pcbs", x => x.PcbId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    EmailAddress = table.Column<string>(type: "TEXT", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    DateLockedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsEmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateEmailConfirmedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsEmailSubscribed = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmailConfirmationToken = table.Column<string>(type: "TEXT", nullable: true),
                    ProfileImage = table.Column<byte[]>(type: "BLOB", nullable: true),
                    DateLastLoginUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateLastActiveUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    ReCaptchaScore = table.Column<double>(type: "REAL", nullable: true),
                    Ip = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L),
                    EmailConfirmedIp = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L),
                    LastSetPasswordIp = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "OAuthCredentials",
                columns: table => new
                {
                    OAuthCredentialId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccessToken = table.Column<string>(type: "TEXT", nullable: true),
                    RefreshToken = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    Ip = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L),
                    DateExpiresUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthCredentials", x => x.OAuthCredentialId);
                    table.ForeignKey(
                        name: "FK_OAuthCredentials_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OAuthRequests",
                columns: table => new
                {
                    OAuthRequestId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Provider = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthorizationReceived = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    AuthorizationCode = table.Column<string>(type: "TEXT", nullable: true),
                    Error = table.Column<string>(type: "TEXT", nullable: true),
                    ErrorDescription = table.Column<string>(type: "TEXT", nullable: true),
                    ReturnToUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    Ip = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthRequests", x => x.OAuthRequestId);
                    table.ForeignKey(
                        name: "FK_OAuthRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartTypes",
                columns: table => new
                {
                    PartTypeId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    ParentPartTypeId = table.Column<long>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartTypes", x => x.PartTypeId);
                    table.ForeignKey(
                        name: "FK_PartTypes_PartTypes_ParentPartTypeId",
                        column: x => x.ParentPartTypeId,
                        principalTable: "PartTypes",
                        principalColumn: "PartTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartTypes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", nullable: true),
                    Color = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_Projects_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserIntegrationConfigurations",
                columns: table => new
                {
                    UserIntegrationConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    SwarmEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    SwarmApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    SwarmApiUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SwarmTimeout = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    DigiKeyEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
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
                    OctopartEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    OctopartClientId = table.Column<string>(type: "TEXT", nullable: true),
                    OctopartClientSecret = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIntegrationConfigurations", x => x.UserIntegrationConfigurationId);
                    table.ForeignKey(
                        name: "FK_UserIntegrationConfigurations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserLoginHistory",
                columns: table => new
                {
                    UserLoginHistoryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    EmailAddress = table.Column<string>(type: "TEXT", nullable: true),
                    IsSuccessful = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanLogin = table.Column<bool>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    ReCaptchaScore = table.Column<double>(type: "REAL", nullable: true),
                    Ip = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLoginHistory", x => x.UserLoginHistoryId);
                    table.ForeignKey(
                        name: "FK_UserLoginHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPrinterConfigurations",
                columns: table => new
                {
                    UserPrinterConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    RemoteAddressUrl = table.Column<string>(type: "TEXT", nullable: true),
                    PrinterName = table.Column<string>(type: "TEXT", nullable: false),
                    PartLabelName = table.Column<string>(type: "TEXT", nullable: false),
                    PartLabelSource = table.Column<int>(type: "INTEGER", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrinterConfigurations", x => x.UserPrinterConfigurationId);
                    table.ForeignKey(
                        name: "FK_UserPrinterConfigurations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserToken",
                columns: table => new
                {
                    UserTokenId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    TokenTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    ReplacedByToken = table.Column<string>(type: "TEXT", nullable: true),
                    DateExpiredUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateRevokedUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    Ip = table.Column<long>(type: "INTEGER", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToken", x => x.UserTokenId);
                    table.ForeignKey(
                        name: "FK_UserToken_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parts",
                columns: table => new
                {
                    PartId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<long>(type: "INTEGER", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "INTEGER", nullable: false),
                    Cost = table.Column<double>(type: "decimal(18,4)", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: true),
                    PartNumber = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    DigiKeyPartNumber = table.Column<string>(type: "TEXT", nullable: true),
                    MouserPartNumber = table.Column<string>(type: "TEXT", nullable: true),
                    ArrowPartNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    PartTypeId = table.Column<long>(type: "INTEGER", nullable: false),
                    MountingTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    PackageType = table.Column<string>(type: "TEXT", nullable: true),
                    ProductUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    LowestCostSupplier = table.Column<string>(type: "TEXT", nullable: true),
                    LowestCostSupplierUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ProjectId = table.Column<long>(type: "INTEGER", nullable: true),
                    Keywords = table.Column<string>(type: "TEXT", nullable: true),
                    DatasheetUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", nullable: true),
                    BinNumber = table.Column<string>(type: "TEXT", nullable: true),
                    BinNumber2 = table.Column<string>(type: "TEXT", nullable: true),
                    Manufacturer = table.Column<string>(type: "TEXT", nullable: true),
                    ManufacturerPartNumber = table.Column<string>(type: "TEXT", nullable: true),
                    SwarmPartNumberManufacturerId = table.Column<int>(type: "INTEGER", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.PartId);
                    table.ForeignKey(
                        name: "FK_Parts_PartTypes_PartTypeId",
                        column: x => x.PartTypeId,
                        principalTable: "PartTypes",
                        principalColumn: "PartTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPcbAssignments",
                columns: table => new
                {
                    ProjectPcbAssignmentId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<long>(type: "INTEGER", nullable: false),
                    PcbId = table.Column<long>(type: "INTEGER", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPcbAssignments", x => x.ProjectPcbAssignmentId);
                    table.ForeignKey(
                        name: "FK_ProjectPcbAssignments_Pcbs_PcbId",
                        column: x => x.PcbId,
                        principalTable: "Pcbs",
                        principalColumn: "PcbId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectPcbAssignments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPrinterTemplateConfigurations",
                columns: table => new
                {
                    UserPrinterTemplateConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserPrinterConfigurationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Line = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    FontName = table.Column<string>(type: "TEXT", nullable: false),
                    AutoSize = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpperCase = table.Column<bool>(type: "INTEGER", nullable: false),
                    LowerCase = table.Column<bool>(type: "INTEGER", nullable: false),
                    FontSize = table.Column<int>(type: "INTEGER", nullable: false),
                    Barcode = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rotate = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    MarginTop = table.Column<int>(type: "INTEGER", nullable: false),
                    MarginBottom = table.Column<int>(type: "INTEGER", nullable: false),
                    MarginLeft = table.Column<int>(type: "INTEGER", nullable: false),
                    MarginRight = table.Column<int>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrinterTemplateConfigurations", x => x.UserPrinterTemplateConfigurationId);
                    table.ForeignKey(
                        name: "FK_UserPrinterTemplateConfigurations_UserPrinterConfigurations_UserPrinterConfigurationId",
                        column: x => x.UserPrinterConfigurationId,
                        principalTable: "UserPrinterConfigurations",
                        principalColumn: "UserPrinterConfigurationId");
                    table.ForeignKey(
                        name: "FK_UserPrinterTemplateConfigurations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "PartSuppliers",
                columns: table => new
                {
                    PartSupplierId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    SupplierPartNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Cost = table.Column<double>(type: "decimal(18,4)", nullable: true),
                    QuantityAvailable = table.Column<int>(type: "INTEGER", nullable: false),
                    MinimumOrderQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartSuppliers", x => x.PartSupplierId);
                    table.ForeignKey(
                        name: "FK_PartSuppliers_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartSuppliers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPartAssignments",
                columns: table => new
                {
                    ProjectPartAssignmentId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<long>(type: "INTEGER", nullable: false),
                    PartId = table.Column<long>(type: "INTEGER", nullable: true),
                    PcbId = table.Column<long>(type: "INTEGER", nullable: true),
                    PartName = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityAvailable = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    ReferenceId = table.Column<string>(type: "TEXT", nullable: true),
                    SchematicReferenceId = table.Column<string>(type: "TEXT", nullable: true),
                    CustomDescription = table.Column<string>(type: "TEXT", nullable: true),
                    Cost = table.Column<double>(type: "REAL", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPartAssignments", x => x.ProjectPartAssignmentId);
                    table.ForeignKey(
                        name: "FK_ProjectPartAssignments_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectPartAssignments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoredFiles",
                columns: table => new
                {
                    StoredFileId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", nullable: false),
                    StoredFileType = table.Column<int>(type: "INTEGER", nullable: false),
                    PartId = table.Column<long>(type: "INTEGER", nullable: true),
                    FileLength = table.Column<int>(type: "INTEGER", nullable: false),
                    Crc32 = table.Column<int>(type: "INTEGER", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.StoredFileId);
                    table.ForeignKey(
                        name: "FK_StoredFiles_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "PartId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PcbStoredFileAssignments",
                columns: table => new
                {
                    PcbStoredFileAssignmentId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PcbId = table.Column<long>(type: "INTEGER", nullable: false),
                    StoredFileId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    DateModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "getutcdate()"),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcbStoredFileAssignments", x => x.PcbStoredFileAssignmentId);
                    table.ForeignKey(
                        name: "FK_PcbStoredFileAssignments_Pcbs_PcbId",
                        column: x => x.PcbId,
                        principalTable: "Pcbs",
                        principalColumn: "PcbId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PcbStoredFileAssignments_StoredFiles_StoredFileId",
                        column: x => x.StoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "StoredFileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OAuthCredentials_UserId",
                table: "OAuthCredentials",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OAuthRequests_UserId",
                table: "OAuthRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_BinNumber_UserId",
                table: "Parts",
                columns: new[] { "BinNumber", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_BinNumber2_UserId",
                table: "Parts",
                columns: new[] { "BinNumber2", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Description_UserId",
                table: "Parts",
                columns: new[] { "Description", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_DigiKeyPartNumber_UserId",
                table: "Parts",
                columns: new[] { "DigiKeyPartNumber", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Keywords_UserId",
                table: "Parts",
                columns: new[] { "Keywords", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Location_UserId",
                table: "Parts",
                columns: new[] { "Location", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_Manufacturer_UserId",
                table: "Parts",
                columns: new[] { "Manufacturer", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_ManufacturerPartNumber_UserId",
                table: "Parts",
                columns: new[] { "ManufacturerPartNumber", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_MouserPartNumber_UserId",
                table: "Parts",
                columns: new[] { "MouserPartNumber", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartNumber_UserId",
                table: "Parts",
                columns: new[] { "PartNumber", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartTypeId_UserId",
                table: "Parts",
                columns: new[] { "PartTypeId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_ProjectId",
                table: "Parts",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_UserId",
                table: "Parts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartSuppliers_PartId",
                table: "PartSuppliers",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_PartSuppliers_UserId",
                table: "PartSuppliers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartTypes_Name_UserId",
                table: "PartTypes",
                columns: new[] { "Name", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_PartTypes_ParentPartTypeId",
                table: "PartTypes",
                column: "ParentPartTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartTypes_UserId",
                table: "PartTypes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PcbStoredFileAssignments_PcbId",
                table: "PcbStoredFileAssignments",
                column: "PcbId");

            migrationBuilder.CreateIndex(
                name: "IX_PcbStoredFileAssignments_StoredFileId",
                table: "PcbStoredFileAssignments",
                column: "StoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPartAssignments_PartId",
                table: "ProjectPartAssignments",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPartAssignments_ProjectId",
                table: "ProjectPartAssignments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPcbAssignments_PcbId",
                table: "ProjectPcbAssignments",
                column: "PcbId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPcbAssignments_ProjectId",
                table: "ProjectPcbAssignments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name_UserId",
                table: "Projects",
                columns: new[] { "Name", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UserId",
                table: "Projects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_PartId",
                table: "StoredFiles",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_UserIntegrationConfigurations_UserId",
                table: "UserIntegrationConfigurations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLoginHistory_UserId",
                table: "UserLoginHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrinterConfigurations_UserId",
                table: "UserPrinterConfigurations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrinterTemplateConfigurations_UserId",
                table: "UserPrinterTemplateConfigurations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrinterTemplateConfigurations_UserPrinterConfigurationId",
                table: "UserPrinterTemplateConfigurations",
                column: "UserPrinterConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailAddress",
                table: "Users",
                column: "EmailAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                table: "Users",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_UserToken_UserId",
                table: "UserToken",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OAuthCredentials");

            migrationBuilder.DropTable(
                name: "OAuthRequests");

            migrationBuilder.DropTable(
                name: "PartSuppliers");

            migrationBuilder.DropTable(
                name: "PcbStoredFileAssignments");

            migrationBuilder.DropTable(
                name: "ProjectPartAssignments");

            migrationBuilder.DropTable(
                name: "ProjectPcbAssignments");

            migrationBuilder.DropTable(
                name: "UserIntegrationConfigurations");

            migrationBuilder.DropTable(
                name: "UserLoginHistory");

            migrationBuilder.DropTable(
                name: "UserPrinterTemplateConfigurations");

            migrationBuilder.DropTable(
                name: "UserToken");

            migrationBuilder.DropTable(
                name: "StoredFiles");

            migrationBuilder.DropTable(
                name: "Pcbs");

            migrationBuilder.DropTable(
                name: "UserPrinterConfigurations");

            migrationBuilder.DropTable(
                name: "Parts");

            migrationBuilder.DropTable(
                name: "PartTypes");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
