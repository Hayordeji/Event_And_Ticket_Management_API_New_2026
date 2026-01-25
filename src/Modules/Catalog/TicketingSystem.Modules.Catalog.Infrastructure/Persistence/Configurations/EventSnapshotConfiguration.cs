using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Entities;

namespace TicketingSystem.Modules.Catalog.Infrastructure.Persistence.Configurations
{
    public class EventSnapshotConfiguration : IEntityTypeConfiguration<EventSnapshot>
    {
        public void Configure(EntityTypeBuilder<EventSnapshot> builder)
        {
            builder.ToTable("EventSnapshots");

            // Primary Key
            builder.HasKey(s => s.Id);

            // Properties
            builder.Property(s => s.EventId)
                .IsRequired();

            builder.Property(s => s.SnapshotVersion)
                .IsRequired();

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(s => s.StartDate)
                .IsRequired();

            builder.Property(s => s.EndDate)
                .IsRequired();

            builder.Property(s => s.ImageUrl)
                .HasMaxLength(500);


            builder.Property(s => s.SnapshotCreatedAt)
                .IsRequired();

            // Venue Value Object as Owned Entity (frozen state)
            builder.OwnsOne(s => s.Venue, venueBuilder =>
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

            // Unique Constraint: One snapshot per (EventId, Version) combination
            builder.HasIndex(s => new { s.EventId, s.SnapshotVersion })
                .IsUnique()
                .HasDatabaseName("UQ_EventSnapshots_EventId_Version");

            // Index for querying snapshots by event
            builder.HasIndex(s => s.EventId)
                .HasDatabaseName("IX_EventSnapshots_EventId");

            // Index for timestamp queries
            builder.HasIndex(s => s.SnapshotCreatedAt)
                .HasDatabaseName("IX_EventSnapshots_SnapshotCreatedAt");

            // No soft delete for snapshots - they are permanent audit trail
            // Explicitly exclude from soft delete global query filter
            builder.HasQueryFilter(s => true); // Override BaseDbContext soft delete filter

            // Audit fields from Entity base class still apply (CreatedAt)
            // But IsDeleted, DeletedAt, DeletedBy are ignored for snapshots
        }
    }
}
