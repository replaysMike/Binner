using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class PartSupplierConfiguration : IEntityTypeConfiguration<PartSupplier>
    {
        public void Configure(EntityTypeBuilder<PartSupplier> builder)
        {
#if INITIALCREATE
            builder.HasOne(p => p.User)
                .WithMany(b => b.PartSuppliers)
                .OnDelete(DeleteBehavior.SetNull);
#endif
            builder.HasOne(p => p.Part)
                .WithMany(b => b.PartSuppliers)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.Cost)
                .HasColumnType("decimal(18,4)");
            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
