using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class ProjectPcbAssignmentConfiguration : IEntityTypeConfiguration<ProjectPcbAssignment>
    {
        public void Configure(EntityTypeBuilder<ProjectPcbAssignment> builder)
        {
#if INITIALCREATE

            builder.HasOne(p => p.User)
                .WithMany(p => p.ProjectPcbAssignments)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
#endif
            builder.HasOne(p => p.Pcb)
                .WithMany(b => b.ProjectPcbAssignments)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(p => p.Project)
                .WithMany(b => b.ProjectPcbAssignments)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
