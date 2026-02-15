using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Domain.Entitites;
using TicketingSystem.Modules.Fulfillment.Domain.Enums;

namespace TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence.Configurations
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.ToTable("Tickets", "fulfillment");

            builder.HasKey(t => t.Id);

            // Ticket Number (unique, indexed)
            builder.Property(t => t.TicketNumber)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(t => t.TicketNumber)
                .IsUnique();

            // Order and Customer References
            builder.Property(t => t.OrderId)
                .IsRequired();

            builder.Property(t => t.OrderNumber)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(t => t.OrderId);
            builder.HasIndex(t => t.OrderNumber);

            builder.Property(t => t.CustomerId)
                .IsRequired();

            builder.HasIndex(t => t.CustomerId);

            // Event References
            builder.Property(t => t.EventId)
                .IsRequired();

            builder.Property(t => t.TicketTypeId)
                .IsRequired();

            builder.HasIndex(t => t.EventId);

            // Event Snapshot Fields
            builder.Property(t => t.EventName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.EventStartDate)
                .IsRequired();

            builder.Property(t => t.EventEndDate)
                .IsRequired();

            builder.Property(t => t.VenueName)
                .HasMaxLength(200);

            builder.Property(t => t.VenueAddress)
                .HasMaxLength(500);

            builder.Property(t => t.VenueCity)
                .HasMaxLength(100);

            builder.Property(t => t.TicketTypeName)
                .IsRequired()
                .HasMaxLength(200);

            // QR Code and Barcode
            builder.Property(t => t.QrCodeData)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasIndex(t => t.QrCodeData)
                .IsUnique();

            builder.Property(t => t.Barcode)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(t => t.Barcode);

            // Status
            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<TicketStatus>(v))
                .HasMaxLength(20);

            builder.HasIndex(t => t.Status);

            // Usage Tracking
            builder.Property(t => t.UsedAt);
            builder.Property(t => t.ScannedBy);
            builder.Property(t => t.ScanLocation)
                .HasMaxLength(200);

            builder.Property(t => t.CancelledAt);
            builder.Property(t => t.CancellationReason)
                .HasMaxLength(500);

            // Customer Information
            builder.Property(t => t.CustomerEmail)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(t => t.CustomerFirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.CustomerLastName)
                .IsRequired()
                .HasMaxLength(100);

            // Pricing
            builder.Property(t => t.PricePaid)
                .IsRequired()
                .HasColumnType("decimal(19, 4)");

            builder.Property(t => t.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("NGN");

            // Audit Fields
            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.UpdatedAt);

            builder.Property(t => t.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(t => t.DeletedAt);

            // Ignore domain events
            builder.Ignore(t => t.DomainEvents);
        }
    }
}
