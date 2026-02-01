using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Domain.Entities;

namespace TicketingSystem.Modules.Sales.Infrastructure.Persistence.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(i => i.Id);

            //// Foreign Key to Event (explicit configuration)
            builder.Property<Guid>("EventId")
                .IsRequired();


            builder.Property(i => i.TicketTypeId)
                .IsRequired();

           
            builder.HasIndex(i => i.TicketTypeId);

            builder.Property(i => i.Quantity)
                .IsRequired();

            builder.OwnsOne(o => o.UnitPrice, priceBuilder =>
            {
                priceBuilder.Property(p => p.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(19,4)")
                    .HasColumnName("UnitPriceAmount");

                priceBuilder.Property(p => p.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnName("UnitPriceCurrency");
            });

            builder.OwnsOne(o => o.Subtotal, priceBuilder =>
            {
                priceBuilder.Property(p => p.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(19,4)")
                    .HasColumnName("SubtotalAmount");

                priceBuilder.Property(p => p.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnName("SubtotalCurrency");
            });

           

            builder.Property(i => i.CreatedAt)
                .IsRequired();

            builder.Property(i => i.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(i => i.DeletedAt);

            // Query filter for soft delete
            builder.HasQueryFilter(i => !i.IsDeleted);
        }
    }
}
