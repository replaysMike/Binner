﻿using Binner.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Binner.Data.Configurations
{
    public class UserPrinterConfigurationConfiguration : IEntityTypeConfiguration<UserPrinterConfiguration>
    {
        public void Configure(EntityTypeBuilder<UserPrinterConfiguration> builder)
        {
            builder.HasOne(p => p.User)
                .WithMany(p => p.UserPrinterConfigurations)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(p => p.DateCreatedUtc)
                .HasDefaultValueSql("getutcdate()");
            builder.Property(p => p.DateModifiedUtc)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
