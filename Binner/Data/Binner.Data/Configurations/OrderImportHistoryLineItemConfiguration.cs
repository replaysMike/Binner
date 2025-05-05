using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class OrderImportHistoryLineItemConfiguration : IEntityTypeConfiguration<OrderImportHistoryLineItem>
    {
        public void Configure(EntityTypeBuilder<OrderImportHistoryLineItem> builder)
        {
#if INITIALCREATE
            builder.HasOne(p => p.User)
                .WithMany(b => b.OrderImportHistoryLineItems)
                .OnDelete(DeleteBehavior.SetNull);
#endif
            builder.HasOne(p => p.OrderImportHistory)
                .WithMany(b => b.OrderImportHistoryLineItems)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
