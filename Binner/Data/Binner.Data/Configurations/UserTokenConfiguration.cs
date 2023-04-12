using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
#if INITIALCREATE
    public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
    {
        public void Configure(EntityTypeBuilder<UserToken> builder)
        {
            builder.HasOne(p => p.User)
                .WithMany(p => p.UserTokens)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.Ip)
                .HasDefaultValue(0);
        }
    }
#endif
}
