using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.ValueObjects;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Domain.Entities
{
     ///<summary>
/// Immutable snapshot of an event at a specific point in time.
/// Created when the first ticket is sold to preserve what customers purchased.
/// </summary>
    public sealed class EventSnapshot : Entity
    {
        public Guid EventId { get; private set; }
        public int SnapshotVersion { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Venue Venue { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public string? ImageUrl { get; private set; }
        public DateTime SnapshotCreatedAt { get; private set; }

        // EF Core navigation
        public Event Event { get; private set; } = null!;

        // EF Core constructor
        private EventSnapshot() { }

        internal EventSnapshot(
            Guid eventId,
            int snapshotVersion,
            string name,
            string description,
            Venue venue,
            DateTime startDate,
            DateTime? endDate,
            string? imageUrl)
        {
            EventId = eventId;
            SnapshotVersion = snapshotVersion;
            Name = name;
            Description = description;
            Venue = venue;
            StartDate = startDate;
            EndDate = endDate;
            ImageUrl = imageUrl;
            SnapshotCreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method to create a snapshot from an event
        /// </summary>
        public static EventSnapshot CreateFrom(Event @event, int snapshotVersion)
        {
            return new EventSnapshot(
                @event.Id,
                snapshotVersion,
                @event.Name,
                @event.Description,
                @event.Venue,
                @event.StartDate,
                @event.EndDate,
                @event.ImageUrl);
        }
    }
}
