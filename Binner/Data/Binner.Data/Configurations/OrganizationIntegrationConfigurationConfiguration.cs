using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
#if INITIALCREATE
    public class OrganizationIntegrationConfigurationConfiguration : IEntityTypeConfiguration<OrganizationIntegrationConfiguration>
    {
        public void Configure(EntityTypeBuilder<OrganizationIntegrationConfiguration> builder)
        {
            builder.HasOne(p => p.Organization)
                .WithMany(p => p.OrganizationIntegrationConfigurations)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
#endif
}
