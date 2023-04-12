using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
#if INITIALCREATE
    public class UserIntegrationConfigurationConfiguration : IEntityTypeConfiguration<UserIntegrationConfiguration>
    {
        public void Configure(EntityTypeBuilder<UserIntegrationConfiguration> builder)
        {
            builder.HasOne(p => p.User)
                .WithMany(p => p.UserIntegrationConfigurations)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
#endif
}
