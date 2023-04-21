using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class ProjectPcbProduceHistoryConfiguration : IEntityTypeConfiguration<ProjectPcbProduceHistory>
    {
        public void Configure(EntityTypeBuilder<ProjectPcbProduceHistory> builder)
        {
#if INITIALCREATE
            builder.HasOne(p => p.User)
                .WithMany(b => b.ProjectPcbProduceHistory)
                .OnDelete(DeleteBehavior.SetNull);
#endif
            builder.HasOne(p => p.ProjectProduceHistory)
                .WithMany(b => b.ProjectPcbProduceHistory)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
