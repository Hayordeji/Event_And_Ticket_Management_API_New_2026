using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Entities;

namespace TicketingSystem.Modules.Catalog.Infrastructure.Persistence.Configurations
{
    public class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
    {
        public void Configure(EntityTypeBuilder<TicketType> builder)
        {
            builder.ToTable("TicketTypes");

            // Primary Key
            builder.HasKey(t => t.Id);

            // Foreign Key to Event (explicit configuration)
            builder.Property<Guid>("EventId")
                .IsRequired();

            // Properties
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Description)
                .HasMaxLength(500);

            builder.Property(t => t.TotalCapacity)
                .IsRequired();

            builder.Property(t => t.ReservedCount)
                .IsRequired();

            builder.Property(t => t.SoldCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(t => t.SaleStartDate)
                .IsRequired();

            builder.Property(t => t.SaleEndDate)
                .IsRequired();

            builder.Property(t => t.MinPurchaseQuantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(t => t.MaxPurchaseQuantity)
                .IsRequired()
                .HasDefaultValue(10);

            builder.Property(t => t.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            

            // Price Value Object as Owned Entity
            builder.OwnsOne(t => t.Price, priceBuilder =>
            {
                priceBuilder.Property(p => p.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(19,4)")
                    .HasColumnName("PriceAmount");

                priceBuilder.Property(p => p.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnName("PriceCurrency");
            });

            // Unique Constraint: One ticket type name per event
            builder.HasIndex("EventId", nameof(TicketType.Name))
                .IsUnique()
                .HasDatabaseName("UQ_TicketTypes_EventId_Name");

            // Index for querying ticket types by event
            builder.HasIndex("EventId")
                .HasDatabaseName("IX_TicketTypes_EventId");

            // Index for active ticket types
            builder.HasIndex(t => t.IsActive)
                .HasDatabaseName("IX_TicketTypes_IsActive");

           

            // Index for sales window queries
            builder.HasIndex(t => new { t.SaleStartDate, t.SaleEndDate })
                .HasDatabaseName("IX_TicketTypes_SalesWindow");

            // Foreign Key Configuration
            // Use Restrict instead of Cascade to avoid SQL Server multiple cascade path error
            // When soft delete is enabled on both parent and child entities
            builder.HasOne<Event>()
                .WithMany(e => e.TicketTypes)
                .HasForeignKey("EventId")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
