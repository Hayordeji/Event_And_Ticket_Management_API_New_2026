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
            builder.ToTable("RefreshTokens");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Token)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(r => r.DeviceInfo)
                .HasMaxLength(512);

            builder.Property(r => r.ExpiresAt).IsRequired();
            builder.Property(r => r.CreatedAt).IsRequired();
            builder.Property(r => r.RevokedAt);

            // Fast token lookup — used on every refresh request
            builder.HasIndex(r => r.Token).IsUnique();

            // UserId is a plain Guid — no FK to AspNetUsers
            // (no cross-module FK rule applies within the same module too for consistency)
            builder.Property(r => r.UserId).IsRequired();
            builder.HasIndex(r => r.UserId);
        }
    }
}
