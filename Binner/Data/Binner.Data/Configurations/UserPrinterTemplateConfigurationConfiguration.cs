using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
#if INITIALCREATE
    public class UserPrinterTemplateConfigurationConfiguration : IEntityTypeConfiguration<UserPrinterTemplateConfiguration>
    {
        public void Configure(EntityTypeBuilder<UserPrinterTemplateConfiguration> builder)
        {
            builder.HasOne(p => p.User)
                .WithMany(p => p.UserPrinterTemplateConfigurations)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.UserPrinterConfiguration)
                .WithMany(p => p.UserPrinterTemplateConfigurations)
                .HasForeignKey(p => p.UserPrinterConfigurationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
#endif
}
