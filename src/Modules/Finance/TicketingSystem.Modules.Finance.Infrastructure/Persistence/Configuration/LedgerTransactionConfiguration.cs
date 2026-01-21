using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Entities;

namespace TicketingSystem.Modules.Finance.Infrastructure.Persistence.Configuration
{
    public class LedgerTransactionConfiguration : IEntityTypeConfiguration<LedgerTransaction>
    {
        public void Configure(EntityTypeBuilder<LedgerTransaction> builder)
        {
            builder.ToTable("LedgerTransactions", "finance");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.ReferenceType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(t => t.ReferenceId)
                .IsRequired();

            // Unique constraint: One transaction per reference
            builder.HasIndex(t => new { t.ReferenceType, t.ReferenceId })
                .IsUnique()
                .HasDatabaseName("IX_LedgerTransactions_Reference");

            builder.Property(t => t.Description)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(t => t.OccurredAt)
                .IsRequired();

            builder.Property(t => t.IsPosted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(t => t.PostedAt);

            // Relationship with entries
            builder.HasMany(t => t.Entries)
                .WithOne()
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Audit fields
            builder.Property(t => t.CreatedAt).IsRequired();
            builder.Property(t => t.UpdatedAt);
            builder.Property(t => t.IsDeleted).IsRequired().HasDefaultValue(false);

            // Ignore domain events
            builder.Ignore(t => t.DomainEvents);

            // Index for querying by reference
            builder.HasIndex(t => t.ReferenceType)
                .HasDatabaseName("IX_LedgerTransactions_ReferenceType");

            builder.HasIndex(t => t.OccurredAt)
                .HasDatabaseName("IX_LedgerTransactions_OccurredAt");
        }
    }
}
