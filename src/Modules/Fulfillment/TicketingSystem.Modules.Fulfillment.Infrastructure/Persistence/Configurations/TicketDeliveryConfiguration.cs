using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TicketingSystem.Modules.Fulfillment.Domain.Entitites;
using TicketingSystem.Modules.Fulfillment.Domain.Enums;

namespace TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence.Configurations
{
    public class TicketDeliveryConfiguration : IEntityTypeConfiguration<TicketDelivery>
    {
        public void Configure(EntityTypeBuilder<TicketDelivery> builder)
        {
            builder.ToTable("TicketDeliveries", "fulfillment");

            builder.HasKey(td => td.Id);

            // Order Reference
            builder.Property(td => td.OrderId)
                .IsRequired();

            builder.HasIndex(td => td.OrderId)
                .IsUnique();

            builder.Property(td => td.OrderNumber)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(td => td.CustomerId)
                .IsRequired();

            // Recipient Information
            builder.Property(td => td.RecipientEmail)
                .IsRequired()
                .HasMaxLength(255);

            // Delivery Status
            builder.Property(td => td.Status)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<DeliveryStatus>(v))
                .HasMaxLength(20);

            builder.HasIndex(td => td.Status);

            builder.Property(td => td.Method)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<DeliveryMethod>(v))
                .HasMaxLength(20);

            // Delivery Tracking
            builder.Property(td => td.AttemptCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(td => td.SentAt);
            builder.Property(td => td.DeliveredAt);
            builder.Property(td => td.FailedAt);

            builder.Property(td => td.FailureReason)
                .HasMaxLength(1000);

            // Email Provider Information
            builder.Property(td => td.EmailProvider)
                .HasMaxLength(50);

            builder.Property(td => td.EmailMessageId)
                .HasMaxLength(200);

            builder.Property(td => td.EmailResponse)
                .HasMaxLength(2000);

            // Ticket References (stored as JSON array)
            builder.Property(td => td.TicketIds)
                .IsRequired()
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>())
                .HasColumnType("nvarchar(max)");

            // Audit Fields
            builder.Property(td => td.CreatedAt)
                .IsRequired();

            builder.Property(td => td.UpdatedAt);

            builder.Property(td => td.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(td => td.DeletedAt);
        }
    }
}
