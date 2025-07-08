using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
#if INITIALCREATE
    public class OrganizationConfigurationConfiguration : IEntityTypeConfiguration<Binner.Data.Model.OrganizationConfiguration>
    {
        public void Configure(EntityTypeBuilder<Binner.Data.Model.OrganizationConfiguration> builder)
        {
            builder.HasOne(p => p.Organization)
                .WithMany(p => p.OrganizationConfigurations)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
#endif
}
