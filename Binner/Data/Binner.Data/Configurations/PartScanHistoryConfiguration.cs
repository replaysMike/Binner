using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class PartScanHistoryConfiguration : IEntityTypeConfiguration<PartScanHistory>
    {
        public void Configure(EntityTypeBuilder<PartScanHistory> builder)
        {
#if INITIALCREATE
            builder.HasOne(p => p.User)
                .WithMany(b => b.PartScanHistories)
                .OnDelete(DeleteBehavior.SetNull);
#endif
            builder.HasOne(p => p.Part)
                .WithMany(b => b.PartScanHistories)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
