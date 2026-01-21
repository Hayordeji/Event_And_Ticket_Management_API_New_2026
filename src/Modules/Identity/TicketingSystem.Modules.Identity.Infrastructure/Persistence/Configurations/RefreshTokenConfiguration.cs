using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Domain.Entities;

namespace TicketingSystem.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens", "identity");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.UserId)
                .IsRequired();

            builder.Property(rt => rt.Token)
                .HasMaxLength(500)
                .IsRequired();

            builder.HasIndex(rt => rt.Token)
                .IsUnique()
                .HasDatabaseName("IX_RefreshTokens_Token");

            builder.Property(rt => rt.ExpiresAt)
                .IsRequired();

            builder.Property(rt => rt.IsRevoked)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(rt => rt.RevokedAt);

            builder.Property(rt => rt.DeviceFingerprintHash)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(rt => rt.UserAgent)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(rt => rt.IpAddress)
                .HasMaxLength(50)
                .IsRequired();

            // Audit fields
            builder.Property(rt => rt.CreatedAt)
                .IsRequired();

            builder.Property(rt => rt.UpdatedAt);

            builder.Property(rt => rt.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Indexes for performance
            builder.HasIndex(rt => rt.UserId)
                .HasDatabaseName("IX_RefreshTokens_UserId");

            builder.HasIndex(rt => new { rt.UserId, rt.IsRevoked, rt.ExpiresAt })
                .HasDatabaseName("IX_RefreshTokens_UserId_IsRevoked_ExpiresAt");
        }
    }
}
