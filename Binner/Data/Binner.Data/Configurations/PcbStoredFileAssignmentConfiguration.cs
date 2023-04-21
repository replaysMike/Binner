using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class PcbStoredFileAssignmentConfiguration : IEntityTypeConfiguration<PcbStoredFileAssignment>
    {
        public void Configure(EntityTypeBuilder<PcbStoredFileAssignment> builder)
        {
#if INITIALCREATE
            builder.HasOne(p => p.User)
                .WithMany(p => p.PcbStoredFileAssignments)
                .OnDelete(DeleteBehavior.SetNull);
#endif
            builder.HasOne(p => p.Pcb)
                .WithMany(b => b.PcbStoredFileAssignments)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(p => p.StoredFile)
                .WithMany(b => b.PcbStoredFileAssignments)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
