using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class PartModelConfiguration : IEntityTypeConfiguration<PartModel>
    {
        public void Configure(EntityTypeBuilder<PartModel> builder)
        {
#if INITIALCREATE
            builder.HasOne(p => p.User)
                .WithMany(b => b.PartModels)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
#endif
            builder.HasOne(p => p.Part)
                .WithMany(b => b.PartModels)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");

            builder.HasIndex(p => new { p.PartId, p.OrganizationId });
        }
    }
}
