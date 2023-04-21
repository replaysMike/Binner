using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class ProjectProduceHistoryConfiguration : IEntityTypeConfiguration<ProjectProduceHistory>
    {
        public void Configure(EntityTypeBuilder<ProjectProduceHistory> builder)
        {
#if INITIALCREATE
            builder.HasOne(p => p.User)
                .WithMany(b => b.ProjectProduceHistory)
                .OnDelete(DeleteBehavior.SetNull);
#endif
            builder.HasOne(p => p.Project)
                .WithMany(b => b.ProjectProduceHistory)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
