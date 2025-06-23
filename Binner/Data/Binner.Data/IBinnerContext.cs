using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Binner.Data
{
    public interface IBinnerContext
    {
        DbSet<CustomField> CustomFields { get; set; }
        DbSet<CustomFieldValue> CustomFieldValues { get; set; }
        DbSet<Label> Labels { get; set; }
        DbSet<LabelTemplate> LabelTemplates { get; set; }
        DbSet<OAuthCredential> OAuthCredentials { get; set; }
        DbSet<OAuthRequest> OAuthRequests { get; set; }
        DbSet<OrderImportHistory> OrderImportHistories { get; set; }
        DbSet<OrderImportHistoryLineItem> OrderImportHistoryLineItems { get; set; }
        DbSet<Organization> Organizations { get; set; }
        DbSet<PartModel> PartModels { get; set; }
        DbSet<PartParametric> PartParametrics { get; set; }
        DbSet<Part> Parts { get; set; }
        DbSet<PartScanHistory> PartScanHistories { get; set; }
        DbSet<PartSupplier> PartSuppliers { get; set; }
        DbSet<PartType> PartTypes { get; set; }
        DbSet<Pcb> Pcbs { get; set; }
        DbSet<PcbStoredFileAssignment> PcbStoredFileAssignments { get; set; }
        DbSet<ProjectPartAssignment> ProjectPartAssignments { get; set; }
        DbSet<ProjectPcbAssignment> ProjectPcbAssignments { get; set; }
        DbSet<ProjectPcbProduceHistory> ProjectPcbProduceHistory { get; set; }
        DbSet<ProjectProduceHistory> ProjectProduceHistory { get; set; }
        DbSet<Project> Projects { get; set; }
        DbSet<StoredFile> StoredFiles { get; set; }
        DbSet<UserIntegrationConfiguration> UserIntegrationConfigurations { get; set; }
        DbSet<UserLoginHistory> UserLoginHistory { get; set; }
        DbSet<UserPrinterConfiguration> UserPrinterConfigurations { get; set; }
        DbSet<UserPrinterTemplateConfiguration> UserPrinterTemplateConfigurations { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<UserToken> UserTokens { get; set; }
    }
}