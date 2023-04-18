using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Binner.Data
{
    public class BinnerContext : DbContext
    {
        /// <summary>
        /// OAuth credentials for integration providers
        /// </summary>
        public DbSet<OAuthCredential> OAuthCredentials { get; set; } = null!;

        /// <summary>
        /// OAuth authentication requests
        /// </summary>
        public DbSet<OAuthRequest> OAuthRequests { get; set; } = null!;

        /// <summary>
        /// Parts
        /// </summary>
        public DbSet<Part> Parts { get; set; } = null!;

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
        /// Projects
        /// </summary>
        public DbSet<Project> Projects { get; set; } = null!;


        /// <summary>
        /// Stored files (user uploaded files)
        /// </summary>
        public DbSet<StoredFile> StoredFiles { get; set; } = null!;

#if INITIALCREATE
        /// <summary>
        /// Users
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// User login history
        /// </summary>
        public DbSet<UserLoginHistory> UserLoginHistory { get; set; } = null!;

        /// <summary>
        /// User integration settings
        /// </summary>
        public DbSet<UserIntegrationConfiguration> UserIntegrationConfigurations { get; set; } = null!;

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

        public BinnerContext(DbContextOptions<BinnerContext> options) : base(options)
        {

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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
            
        }
    }
}