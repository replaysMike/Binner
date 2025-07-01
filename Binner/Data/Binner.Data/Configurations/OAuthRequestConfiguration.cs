using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class OAuthRequestConfiguration : IEntityTypeConfiguration<OAuthRequest>
    {
        public void Configure(EntityTypeBuilder<OAuthRequest> builder)
        {
#if INITIALCREATE
            builder.HasOne(p => p.User)
                .WithMany(b => b.OAuthRequests)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Property(p => p.Ip)
                .HasDefaultValue(0);
#endif

            builder.Property(p => p.AuthorizationReceived)
                .HasDefaultValue(false)
                .ValueGeneratedNever();
            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
