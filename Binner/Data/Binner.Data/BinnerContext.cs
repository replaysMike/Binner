using Binner.Data.Converters;
using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Binner.Data
{
    public class BinnerContext : DbContext, IBinnerContext
    {
        /// <summary>
        /// User-defined custom fields
        /// </summary>
        public DbSet<CustomField> CustomFields { get; set; } = null!;

        /// <summary>
        /// Values for CustomFields
        /// </summary>
        public DbSet<CustomFieldValue> CustomFieldValues { get; set; } = null!;

        /// <summary>
        /// Label designs
        /// </summary>
        public DbSet<Label> Labels { get; set; } = null!;

        /// <summary>
        /// Label templates
        /// </summary>
        public DbSet<LabelTemplate> LabelTemplates { get; set; } = null!;

        /// <summary>
        /// Tracks the read state of system messages by user
        /// </summary>
        public DbSet<MessageState> MessageStates { get; set; } = null!;

        /// <summary>
        /// OAuth credentials for integration providers
        /// </summary>
        public DbSet<OAuthCredential> OAuthCredentials { get; set; } = null!;

        /// <summary>
        /// OAuth authentication requests
        /// </summary>
        public DbSet<OAuthRequest> OAuthRequests { get; set; } = null!;

        /// <summary>
        /// History of imported orders
        /// </summary>
        public DbSet<OrderImportHistory> OrderImportHistories { get; set; } = null!;

        /// <summary>
        /// Individual line items associated with an Order Import History
        /// </summary>
        public DbSet<OrderImportHistoryLineItem> OrderImportHistoryLineItems { get; set; } = null!;

        /// <summary>
        /// Parts
        /// </summary>
        public DbSet<Part> Parts { get; set; } = null!;

        /// <summary>
        /// Part cad models
        /// </summary>
        public DbSet<PartModel> PartModels { get; set; } = null!;

        /// <summary>
        /// Part parametrics
        /// </summary>
        public DbSet<PartParametric> PartParametrics { get; set; } = null!;

        /// <summary>
        /// History of barcode scanned labels
        /// </summary>
        public DbSet<PartScanHistory> PartScanHistories { get; set; } = null!;

        /// <summary>
        /// Part suppliers (manually created suppliers by user)
        /// </summary>
        public DbSet<PartSupplier> PartSuppliers { get; set; } = null!;

        /// <summary>
        /// Part types
        /// </summary>
        public DbSet<PartType> PartTypes { get; set; } = null!;

        /// <summary>
        /// Printed circuit boards (BOM)
        /// </summary>
        public DbSet<Pcb> Pcbs { get; set; } = null!;

        /// <summary>
        /// PCB stored files (BOM)
        /// </summary>
        public DbSet<PcbStoredFileAssignment> PcbStoredFileAssignments { get; set; } = null!;

        /// <summary>
        /// Project part assignments (BOM)
        /// </summary>
        public DbSet<ProjectPartAssignment> ProjectPartAssignments { get; set; } = null!;

        /// <summary>
        /// Project PCB assignments (BOM)
        /// </summary>
        public DbSet<ProjectPcbAssignment> ProjectPcbAssignments { get; set; } = null!;

        /// <summary>
        /// Produce history (BOM)
        /// </summary>
        public DbSet<ProjectProduceHistory> ProjectProduceHistory { get; set; } = null!;

        /// <summary>
        /// Produce history by pcb (BOM)
        /// </summary>
        public DbSet<ProjectPcbProduceHistory> ProjectPcbProduceHistory { get; set; } = null!;

        /// <summary>
        /// Projects
        /// </summary>
        public DbSet<Project> Projects { get; set; } = null!;


        /// <summary>
        /// Stored files (user uploaded files)
        /// </summary>
        public DbSet<StoredFile> StoredFiles { get; set; } = null!;

        /// <summary>
        /// Organization groups users together and may signify a company or grouping
        /// </summary>
        public DbSet<Organization> Organizations { get; set; } = null!;

        /// <summary>
        /// Organization configurations
        /// </summary>
        public DbSet<OrganizationConfiguration> OrganizationConfigurations { get; set; } = null!;

        /// <summary>
        /// User integration settings
        /// </summary>
        public DbSet<OrganizationIntegrationConfiguration> OrganizationIntegrationConfigurations { get; set; } = null!;

#if INITIALCREATE
        /// <summary>
        /// Users
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// User configurations
        /// </summary>
        public DbSet<UserConfiguration> UserConfigurations { get; set; } = null!;

        /// <summary>
        /// User login history
        /// </summary>
        public DbSet<UserLoginHistory> UserLoginHistory { get; set; } = null!;

        /// <summary>
        /// User printer settings
        /// </summary>
        public DbSet<UserPrinterConfiguration> UserPrinterConfigurations { get; set; } = null!;

        /// <summary>
        /// User printer template settings
        /// </summary>
        public DbSet<UserPrinterTemplateConfiguration> UserPrinterTemplateConfigurations { get; set; } = null!;

        /// <summary>
        /// User tokens
        /// </summary>
        public DbSet<UserToken> UserTokens { get; set; } = null!;

#endif

        public BinnerContext()
        {

        }

        //public BinnerContext(DbContextOptions<BinnerContext> options) : base(options) { }

        public BinnerContext(DbContextOptions options) : base(options)
        {
            // required for descendant DbContext's that inherit from BinnerContext
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // add all entity configuration profiles automatically
            var assembly = Assembly.GetExecutingAssembly();
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
            modelBuilder.HasDefaultSchema("dbo");

            // treat all dates as UTC
            UtcDateTimeModelBuilder.Apply(modelBuilder);
        }

        /*protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HaveConversion<DecimalToDoubleConverter>();
        }*/

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        public virtual async Task<string> GetLicenseKeyAsync(int organizationId)
        {
            return await OrganizationConfigurations
                .Where(x => x.OrganizationId == organizationId)
                .Select(x => x.LicenseKey)
                .FirstOrDefaultAsync() ?? string.Empty;
        }

        public virtual string GetLicenseKey(int organizationId)
        {
            return OrganizationConfigurations
                .Where(x => x.OrganizationId == organizationId)
                .Select(x => x.LicenseKey)
                .FirstOrDefault() ?? string.Empty;
        }
    }
}