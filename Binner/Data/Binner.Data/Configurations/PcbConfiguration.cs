using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class PcbConfiguration : IEntityTypeConfiguration<Pcb>
    {
        public void Configure(EntityTypeBuilder<Pcb> builder)
        {
#if INITIALCREATE
            builder.HasOne(p => p.User)
                .WithMany(p => p.Pcbs)
                .OnDelete(DeleteBehavior.Restrict);
#endif
            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
