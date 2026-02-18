using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel.Outbox;

namespace TicketingSystem.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.EventType)
                .HasMaxLength(512)
                .IsRequired();

            builder.Property(o => o.EventPayload)
                .IsRequired();

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(o => o.OccurredAt).IsRequired();
            builder.Property(o => o.CreatedAt).IsRequired();
            builder.Property(o => o.ProcessedAt);
            builder.Property(o => o.RetryCount).IsRequired();
            builder.Property(o => o.RetryAt);
            builder.Property(o => o.ErrorMessage).HasMaxLength(2000);

            // Index for worker query performance
            builder.HasIndex(o => new { o.Status, o.RetryAt, o.CreatedAt });
        }
    }
}
