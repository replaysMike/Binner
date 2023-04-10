using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class ProjectPcbAssignmentConfiguration : IEntityTypeConfiguration<ProjectPcbAssignment>
    {
        public void Configure(EntityTypeBuilder<ProjectPcbAssignment> builder)
        {
            builder.HasOne(p => p.Pcb)
                .WithMany(b => b.ProjectPcbAssignments)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(p => p.Project)
                .WithMany(b => b.ProjectPcbAssignments)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
