using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
#if INITIALCREATE
    public class UserBarcodeConfigurationConfiguration : IEntityTypeConfiguration<UserBarcodeConfiguration>
    {
        public void Configure(EntityTypeBuilder<UserBarcodeConfiguration> builder)
        {
            builder.HasOne(p => p.User)
                .WithMany(p => p.UserBarcodeConfigurations)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
#endif
}
