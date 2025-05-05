using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class OrderImportHistoryConfiguration : IEntityTypeConfiguration<OrderImportHistory>
    {
        public void Configure(EntityTypeBuilder<OrderImportHistory> builder)
        {
#if INITIALCREATE
            builder.HasOne(p => p.User)
                .WithMany(b => b.OrderImportHistory)
                .OnDelete(DeleteBehavior.SetNull);
#endif

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
