using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
#if INITIALCREATE
    public class UserConfigurationConfiguration : IEntityTypeConfiguration<Model.UserConfiguration>
    {
        public void Configure(EntityTypeBuilder<Model.UserConfiguration> builder)
        {
            builder.HasOne(p => p.User)
                .WithMany(p => p.UserConfigurations)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
#endif
}
