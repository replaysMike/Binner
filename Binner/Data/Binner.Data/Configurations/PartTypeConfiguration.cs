using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class PartTypeConfiguration : IEntityTypeConfiguration<PartType>
    {
        public void Configure(EntityTypeBuilder<PartType> builder)
        {
            builder.HasOne(p => p.User)
                .WithMany(b => b.PartTypes)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(p => p.ParentPartType)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");

            builder.HasIndex(p => new { p.Name, p.UserId });
        }
    }
}
