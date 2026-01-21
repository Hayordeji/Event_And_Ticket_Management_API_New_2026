using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Entities;

namespace TicketingSystem.Modules.Finance.Infrastructure.Persistence.Configuration
{
    public class LedgerAccountConfiguration : IEntityTypeConfiguration<LedgerAccount>
    {
        public void Configure(EntityTypeBuilder<LedgerAccount> builder)
        {
            builder.ToTable("LedgerAccounts", "finance");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.AccountName)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(a => a.AccountName)
                .IsUnique()
                .HasDatabaseName("IX_LedgerAccounts_AccountName");

            builder.Property(a => a.AccountCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(a => a.AccountCode)
                .IsUnique()
                .HasDatabaseName("IX_LedgerAccounts_AccountCode");

            builder.Property(a => a.AccountType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Money value object - stored as two columns
            builder.OwnsOne(a => a.CurrentBalance, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("CurrentBalanceAmount")
                    .HasColumnType("decimal(19,4)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("CurrentBalanceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.Property(a => a.Description)
                .HasMaxLength(500);

            builder.Property(a => a.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Concurrency control
            builder.Property(a => a.RowVersion)
                .IsRowVersion();

            // Audit fields
            builder.Property(a => a.CreatedAt).IsRequired();
            builder.Property(a => a.UpdatedAt);
            builder.Property(a => a.IsDeleted).IsRequired().HasDefaultValue(false);

            // Ignore domain events
            builder.Ignore(a => a.DomainEvents);
        }
    }
}
