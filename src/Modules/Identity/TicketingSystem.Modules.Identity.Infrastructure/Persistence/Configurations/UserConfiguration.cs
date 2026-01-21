using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Domain.Entities;
using TicketingSystem.Modules.Identity.Domain.ValueObjects;

namespace TicketingSystem.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users", "identity");

            builder.HasKey(u => u.Id);

            // Email value object
            builder.Property(u => u.Email)
                .HasConversion(
                    email => email.Value,
                    value => Email.Create(value).Value) // We know it's valid from DB
                .HasMaxLength(255)
                .IsRequired();

            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            // Password hash value object
            builder.Property(u => u.PasswordHash)
                .HasConversion(
                    hash => hash.Value,
                    value => PasswordHash.FromHash(value))
                .HasMaxLength(60) // BCrypt hash length
                .IsRequired();

            // Simple properties
            builder.Property(u => u.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(u => u.IsEmailVerified)
                .IsRequired();

            builder.Property(u => u.EmailVerifiedAt);

            builder.Property(u => u.LastLoginAt);

            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Audit fields (inherited from Entity base class)
            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.UpdatedAt);

            builder.Property(u => u.CreatedBy);

            builder.Property(u => u.UpdatedBy);

            builder.Property(u => u.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(u => u.DeletedAt);

            builder.Property(u => u.DeletedBy);

            // Relationships
            builder.HasMany(u => u.RefreshTokens)
                .WithOne()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore domain events (not persisted)
            builder.Ignore(u => u.DomainEvents);
        }
    }
}
