using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
#if INITIALCREATE
    public class UserLocaleConfigurationConfiguration : IEntityTypeConfiguration<UserLocaleConfiguration>
    {
        public void Configure(EntityTypeBuilder<UserLocaleConfiguration> builder)
        {
            builder.HasOne(p => p.User)
                .WithMany(p => p.UserLocaleConfigurations)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
#endif
}
