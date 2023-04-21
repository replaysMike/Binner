using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class PartConfiguration : IEntityTypeConfiguration<Part>
    {
        public void Configure(EntityTypeBuilder<Part> builder)
        {
#if INITIALCREATE
            builder.HasOne(p => p.User)
                .WithMany(b => b.Parts)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
#endif
            builder.HasOne(p => p.Project)
                .WithMany(b => b.Parts)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(p => p.PartType)
                .WithMany(b => b.Parts)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.Cost)
                .HasColumnType("decimal(18,4)");
            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");

            builder.HasIndex(p => new { p.PartNumber, p.UserId });
            builder.HasIndex(p => new { p.DigiKeyPartNumber, p.UserId });
            builder.HasIndex(p => new { p.MouserPartNumber, p.UserId });
            builder.HasIndex(p => new { p.Description, p.UserId });
            builder.HasIndex(p => new { p.Keywords, p.UserId });
            builder.HasIndex(p => new { p.PartTypeId, p.UserId });
            builder.HasIndex(p => new { p.Location, p.UserId });
            builder.HasIndex(p => new { p.BinNumber, p.UserId });
            builder.HasIndex(p => new { p.BinNumber2, p.UserId });
            builder.HasIndex(p => new { p.Manufacturer, p.UserId });
            builder.HasIndex(p => new { p.ManufacturerPartNumber, p.UserId });
        }
    }
}
