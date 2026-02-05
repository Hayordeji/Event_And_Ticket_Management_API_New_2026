using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Domain.Entities;

namespace TicketingSystem.Modules.Sales.Infrastructure.Persistence.Configurations
{
    internal class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
    {
        public void Configure(EntityTypeBuilder<WebhookEvent> builder)
        {
            builder.ToTable("WebhookEvents");

            builder.HasKey(e => e.Id);

            // Unique constraint on Gateway + GatewayEventId (IDEMPOTENCY)
            builder.HasIndex(e => new { e.Gateway, e.GatewayEventId })
                .IsUnique();

            builder.Property(e => e.GatewayEventId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Gateway)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.PaymentReference)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(e => e.PaymentReference);

            builder.Property(e => e.IsProcessed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.ProcessedAt);

            builder.Property(e => e.RawPayload)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.DeletedAt);

            // Query filter for soft delete
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
