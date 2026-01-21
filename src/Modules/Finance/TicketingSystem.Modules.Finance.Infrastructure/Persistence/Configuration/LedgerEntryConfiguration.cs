using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Entities;

namespace TicketingSystem.Modules.Finance.Infrastructure.Persistence.Configuration
{
    public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
    {
        public void Configure(EntityTypeBuilder<LedgerEntry> builder)
        {
            builder.ToTable("LedgerEntries", "finance");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.TransactionId)
                .IsRequired();

            builder.Property(e => e.AccountId)
                .IsRequired();

            // Money value object
            builder.OwnsOne(e => e.Amount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("Amount")
                    .HasColumnType("decimal(19,4)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.Property(e => e.EntryType)
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            // Relationship with account
            builder.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Audit fields
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

            // Indexes for performance
            builder.HasIndex(e => e.TransactionId)
                .HasDatabaseName("IX_LedgerEntries_TransactionId");

            builder.HasIndex(e => e.AccountId)
                .HasDatabaseName("IX_LedgerEntries_AccountId");

            builder.HasIndex(e => new { e.AccountId, e.EntryType })
                .HasDatabaseName("IX_LedgerEntries_AccountId_EntryType");
        }
    }
}
