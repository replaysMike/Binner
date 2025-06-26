using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Binner.Data
{
    public interface IBinnerContext : IAsyncDisposable
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

        /*
            Required DbContext methods
         */
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();
        ChangeTracker ChangeTracker { get; }
        EntityEntry Entry(object entity);
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class;
        ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
        EntityEntry Attach(object entity);
        EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class;
        EntityEntry Update(object entity);
        EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class;
        EntityEntry Remove(object entity);
        EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class;
        void AddRange(params object[] entities);
        Task AddRangeAsync(params object[] entities);

        void AttachRange(params object[] entities);
        void UpdateRange(params object[] entities);
        void RemoveRange(params object[] entities);
        void AddRange(IEnumerable<object> entities);
        Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default);
        void AttachRange(IEnumerable<object> entities);
        void UpdateRange(IEnumerable<object> entities);
        void RemoveRange(IEnumerable<object> entities);
        IQueryable<TResult> FromExpression<TResult>(Expression<Func<IQueryable<TResult>>> expression);
    }
}