using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Access.Domain.Entities;
using TicketingSystem.Modules.Access.Domain.Enums;

namespace TicketingSystem.Modules.Access.Infrastructure.Persistence.Configuration
{
    public class ScanLogConfiguration : IEntityTypeConfiguration<ScanLog>
    {
        public void Configure(EntityTypeBuilder<ScanLog> builder)
        {
            builder.ToTable("ScanLogs", "access");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.TicketId).IsRequired();
            builder.HasIndex(s => s.TicketId);

            builder.Property(s => s.TicketNumber).IsRequired().HasMaxLength(30);
            builder.HasIndex(s => s.TicketNumber);

            builder.Property(s => s.EventId).IsRequired();
            builder.HasIndex(s => s.EventId);

            builder.Property(s => s.ScannedBy).IsRequired();

            builder.Property(s => s.DeviceId).IsRequired().HasMaxLength(100);

            builder.Property(s => s.GateLocation).IsRequired().HasMaxLength(100);

            builder.Property(s => s.Result)
                .IsRequired()
                .HasConversion(v => v.ToString(), v => Enum.Parse<ScanResult>(v))
                .HasMaxLength(20);

            builder.HasIndex(s => s.Result);

            builder.Property(s => s.DenialReason)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToString() : null,
                    v => v != null ? Enum.Parse<DenialReason>(v) : (DenialReason?)null)
                .HasMaxLength(30);

            builder.Property(s => s.ScannedAt).IsRequired();
            builder.HasIndex(s => s.ScannedAt);

            builder.Property(s => s.CreatedAt).IsRequired();

            builder.Property(s => s.IsDeleted).IsRequired().HasDefaultValue(false);

            builder.HasQueryFilter(s => !s.IsDeleted);

            builder.Ignore(s => s.DomainEvents);
        }
    }
}
