using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
    {
        public void Configure(EntityTypeBuilder<StoredFile> builder)
        {
#if INITIALCREATE
            builder.HasOne(p => p.User)
                .WithMany(p => p.StoredFiles)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
#endif

            builder.HasOne(p => p.Part)
                .WithMany(b => b.StoredFiles)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
