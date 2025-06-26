using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
#if INITIALCREATE
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasOne(p => p.Organization)
                .WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.Ip)
                .HasDefaultValue(0);
            builder.Property(p => p.EmailConfirmedIp)
                .HasDefaultValue(0);
            builder.Property(p => p.LastSetPasswordIp)
                .HasDefaultValue(0);

            builder.HasIndex(p => p.Name);

            builder.HasIndex(p => p.EmailAddress);

            builder.HasIndex(p => p.PhoneNumber);
        }
    }
#endif
}
