using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class PartParametricConfiguration : IEntityTypeConfiguration<PartParametric>
    {
        public void Configure(EntityTypeBuilder<PartParametric> builder)
        {
#if INITIALCREATE
            builder.HasOne(p => p.User)
                .WithMany(b => b.PartParametrics)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
#endif
            builder.HasOne(p => p.Part)
                .WithMany(b => b.PartParametrics)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.ValueNumber)
                .HasColumnType("decimal(18,4)");
            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");

            builder.HasIndex(p => new { p.PartId, p.OrganizationId });
        }
    }
}
