using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Entities;

namespace TicketingSystem.Modules.Catalog.Infrastructure.Persistence.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.ToTable("Events");

            // Primary Key
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(e => e.HostId)
                .IsRequired();

            builder.Property(e => e.StartDate)
                .IsRequired();

            builder.Property(e => e.EndDate)
                .IsRequired();

            builder.Property(e => e.IsPublished)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.PublishedAt);

            builder.Property(e => e.IsCancelled)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.CancelledAt);

            builder.Property(e => e.CancellationReason)
                .HasMaxLength(500);

            builder.Property(e => e.ImageUrl)
                .HasMaxLength(500);

            // Venue Value Object as Owned Entity
            builder.OwnsOne(e => e.Venue, venueBuilder =>
            {
                venueBuilder.Property(v => v.Name)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("VenueName");

                venueBuilder.Property(v => v.Address)
                    .IsRequired()
                    .HasMaxLength(300)
                    .HasColumnName("VenueAddress");

                venueBuilder.Property(v => v.City)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("VenueCity");

                venueBuilder.Property(v => v.State)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("VenueState");

                venueBuilder.Property(v => v.Country)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("VenueCountry");

                venueBuilder.Property(v => v.PostalCode)
                    .HasMaxLength(20)
                    .HasColumnName("VenuePostalCode");

                venueBuilder.Property(v => v.Latitude)
                    .HasPrecision(10, 7)
                    .HasColumnName("VenueLatitude");

                venueBuilder.Property(v => v.Longitude)
                    .HasPrecision(10, 7)
                    .HasColumnName("VenueLongitude");
            });

            // Navigation Properties
            // Use Restrict for TicketTypes to avoid SQL Server cascade path conflicts with soft delete
            //builder.HasMany(e => e.TicketTypes)
            //    .WithOne()
            //    .HasForeignKey("EventId")
            //    .OnDelete(DeleteBehavior.Restrict);

            //builder.HasMany(e => e.Snapshots)
            //    .WithOne()
            //    .HasForeignKey("EventId")
            //    .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(e => e.HostId)
                .HasDatabaseName("IX_Events_HostId");

            builder.HasIndex(e => e.StartDate)
                .HasDatabaseName("IX_Events_StartDate");

            

            builder.HasIndex(e => e.IsPublished)
                .HasDatabaseName("IX_Events_IsPublished");

            // Composite index for common queries (published events by date)
            builder.HasIndex(e => new { e.IsPublished, e.StartDate })
                .HasDatabaseName("IX_Events_IsPublished_StartDate");


        }
    }
}
