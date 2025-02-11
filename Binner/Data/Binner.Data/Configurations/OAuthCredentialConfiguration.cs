using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class OAuthCredentialConfiguration : IEntityTypeConfiguration<OAuthCredential>
    {
        public void Configure(EntityTypeBuilder<OAuthCredential> builder)
        {
#if INITIALCREATE
            builder.HasOne(p => p.User)
                .WithMany(b => b.OAuthCredentials)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.Ip)
                .HasDefaultValue(0);
#endif

            builder.HasIndex(p => new { p.Provider, p.UserId, p.OrganizationId });

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
