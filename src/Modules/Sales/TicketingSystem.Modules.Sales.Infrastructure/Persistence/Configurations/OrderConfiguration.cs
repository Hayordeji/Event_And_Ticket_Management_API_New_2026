using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Domain.Entities;

namespace TicketingSystem.Modules.Sales.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.OwnsOne(o => o.OrderNumber, priceBuilder =>
            {
                priceBuilder.Property(p => p.Value)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasColumnName("OrderNumber")
                    .HasMaxLength(50);
            });

            builder.Property(o => o.CustomerId)
                .IsRequired();

            builder.HasIndex(o => o.CustomerId);

            builder.Property(o => o.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            

            builder.OwnsOne(o => o.TotalAmount, priceBuilder =>
            {
                priceBuilder.Property(p => p.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(19,4)")
                    .HasColumnName("TotalAmount");

                priceBuilder.Property(p => p.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnName("TotalAmountCurrency");
            });

            builder.OwnsOne(o => o.PlatformFee, priceBuilder =>
            {
                priceBuilder.Property(p => p.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(19,4)")
                    .HasColumnName("PlatforFee");

                priceBuilder.Property(p => p.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnName("PlatformFeeCurrency");
            });

            builder.OwnsOne(o => o.GrandTotal, priceBuilder =>
            {
                priceBuilder.Property(p => p.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(19,4)")
                    .HasColumnName("GrandTotalAmount");

                priceBuilder.Property(p => p.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnName("GrandTotalCurrency");
            });

            


            builder.Property(o => o.CancellationReason)
                .HasMaxLength(500);

            builder.Property(o => o.CreatedAt)
                .IsRequired();

            builder.Property(o => o.UpdatedAt);

            builder.Property(o => o.PaidAt);

            builder.Property(o => o.ExpiresAt);

            builder.Property(o => o.CancelledAt);

            builder.Property(o => o.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(o => o.DeletedAt);

            //// Relationships
            //builder.HasMany(o => o.Items)
            //    .WithOne(i => i.Order)
            //    .HasForeignKey(i => i.OrderId)
            //    .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.Payments)
               .WithOne()
               .HasForeignKey("OrderId") // shadow FK if not exposed
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.Payments)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Query filter for soft delete
            builder.HasQueryFilter(o => !o.IsDeleted);
        }
    }
}
