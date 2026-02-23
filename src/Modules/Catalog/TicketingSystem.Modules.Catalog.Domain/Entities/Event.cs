using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Enums;
using TicketingSystem.Modules.Catalog.Domain.Events;
using TicketingSystem.Modules.Catalog.Domain.ValueObjects;
using TicketingSystem.Modules.Finance.Domain.ValueObjects;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Domain.Entities
{
    /// <summary>
/// Event aggregate root - represents a live, mutable event.
/// When tickets are sold, a snapshot is created to preserve what customers purchased.
/// </summary>
    public sealed class Event : AggregateRoot
    {
        public Guid HostId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Venue Venue { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public string? ImageUrl { get; private set; }
        public EventStatus Status { get; private set; }
        public DateTime? PublishedAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public bool IsPublished { get; private set; }
        public bool IsCancelled { get; private set; }


        public string? CancellationReason { get; private set; }

        // Snapshot tracking
        public bool HasSnapshot { get; private set; }
        public int CurrentSnapshotVersion { get; private set; }

        // EF Core navigation
        private readonly List<EventSnapshot> _snapshots = new();
        public IReadOnlyCollection<EventSnapshot> Snapshots => _snapshots.AsReadOnly();

        private readonly List<TicketType> _ticketTypes = new();
        public IReadOnlyCollection<TicketType> TicketTypes => _ticketTypes.AsReadOnly();

        // EF Core constructor
        private Event() { }

        private Event(
            Guid hostId,
            string name,
            string description,
            Venue venue,
            DateTime startDate,
            DateTime? endDate,
            string? imageUrl)
        {
            HostId = hostId;
            Name = name;
            Description = description;
            Venue = venue;
            StartDate = startDate;
            EndDate = endDate;
            ImageUrl = imageUrl;
            Status = EventStatus.Draft;
            HasSnapshot = false;
            CurrentSnapshotVersion = 0;
        }

        /// <summary>
        /// Factory method to create a new event
        /// </summary>
        public static Result<Event> Create(
            Guid hostId,
            string name,
            string description,
            Venue venue,
            DateTime startDate,
            DateTime? endDate = null,
            string? imageUrl = null)
        {
            // Validation
            if (hostId == Guid.Empty)
                return Result.Failure<Event>("Host ID is required");

            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<Event>("Event name is required");

            if (name.Length > 200)
                return Result.Failure<Event>("Event name cannot exceed 200 characters");

            if (string.IsNullOrWhiteSpace(description))
                return Result.Failure<Event>("Event description is required");

            if (description.Length > 5000)
                return Result.Failure<Event>("Event description cannot exceed 5000 characters");

            if (startDate < DateTime.UtcNow.AddHours(-1)) // Allow 1 hour tolerance
                return Result.Failure<Event>("Event start date cannot be in the past");

            if (endDate.HasValue && endDate.Value <= startDate)
                return Result.Failure<Event>("Event end date must be after start date");

            var @event = new Event(
                hostId,
                name.Trim(),
                description.Trim(),
                venue,
                startDate,
                endDate,
                imageUrl?.Trim());

            // Raise domain event
            @event.RaiseDomainEvent(new EventCreatedEvent(
                @event.Id,
                @event.Name,
                hostId,
                startDate,
                DateTime.UtcNow));

            return Result.Success(@event);
        }

        /// <summary>
        /// Publish the event (make it available for ticket sales)
        /// </summary>
        public Result Publish()
        {
            if (Status == EventStatus.Published)
                return Result.Success();

            if (Status != EventStatus.Draft)
                return Result.Failure("Only draft events can be published");

            if (!_ticketTypes.Any())
                return Result.Failure("Cannot publish event without ticket types");

            if (!_ticketTypes.Any(t => t.IsActive))
                return Result.Failure("Cannot publish event without active ticket types");

            Status = EventStatus.Published;
            PublishedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;

            RaiseDomainEvent(new EventPublishedEvent(Id,HostId,Name,PublishedAt.Value, DateTime.UtcNow));

            return Result.Success();
        }

        /// <summary>
        /// Update event details. Creates a snapshot if tickets have been sold.
        /// </summary>
        public Result Update(
            string? name = null,
            string? description = null,
            Venue? venue = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? imageUrl = null,
            bool forceSnapshot = false)
        {
            if (Status == EventStatus.Cancelled)
                return Result.Failure("Cannot update a cancelled event");

            if (Status == EventStatus.Completed)
                return Result.Failure("Cannot update a completed event");

            bool snapshotCreated = false;
            int? newSnapshotVersion = null;

            // If tickets have been sold, create a snapshot before updating
            bool ticketsSold = _ticketTypes.Any(t => t.SoldCount > 0);

            if ((ticketsSold && !HasSnapshot) || forceSnapshot)
            {
                var snapshot = EventSnapshot.CreateFrom(this, CurrentSnapshotVersion + 1);
                _snapshots.Add(snapshot);
                HasSnapshot = true;
                CurrentSnapshotVersion++;
                snapshotCreated = true;
                newSnapshotVersion = CurrentSnapshotVersion;
            }

            // Update fields
            if (name != null)
            {
                if (string.IsNullOrWhiteSpace(name))
                    return Result.Failure("Event name cannot be empty");

                if (name.Length > 200)
                    return Result.Failure("Event name cannot exceed 200 characters");

                Name = name.Trim();
            }

            if (description != null)
            {
                if (string.IsNullOrWhiteSpace(description))
                    return Result.Failure("Event description cannot be empty");

                if (description.Length > 5000)
                    return Result.Failure("Event description cannot exceed 5000 characters");

                Description = description.Trim();
            }

            if (venue != null)
                Venue = venue;

            if (startDate.HasValue)
            {
                if (startDate.Value < DateTime.UtcNow.AddHours(-1))
                    return Result.Failure("Event start date cannot be in the past");

                StartDate = startDate.Value;
            }

            if (endDate.HasValue)
            {
                if (endDate.Value <= StartDate)
                    return Result.Failure("Event end date must be after start date");

                EndDate = endDate;
            }

            if (imageUrl != null)
                ImageUrl = imageUrl.Trim();

            //// Raise domain event
            //RaiseDomainEvent(new EventUpdatedEvent(
            //    Id,
            //    snapshotCreated,
            //    newSnapshotVersion,
            //    DateTime.UtcNow));

            return Result.Success();
        }

        /// <summary>
        /// Cancel the event
        /// </summary>
        public Result Cancel(string reason)
        {
            if (Status == EventStatus.Cancelled)
                return Result.Failure("Event is already cancelled");

            if (Status == EventStatus.Completed)
                return Result.Failure("Cannot cancel a completed event");

            if (string.IsNullOrWhiteSpace(reason))
                return Result.Failure("Cancellation reason is required");

            Status = EventStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            CancellationReason = reason.Trim();

            RaiseDomainEvent(new EventCancelledEvent(Id, reason, DateTime.UtcNow));

            return Result.Success();
        }

        /// <summary>
        /// Mark event as completed (after it has ended)
        /// </summary>
        public Result MarkAsCompleted()
        {
            if (Status == EventStatus.Cancelled)
                return Result.Failure("Cannot complete a cancelled event");

            if (Status == EventStatus.Completed)
                return Result.Failure("Event is already completed");

            if (StartDate > DateTime.UtcNow)
                return Result.Failure("Cannot complete an event that hasn't started yet");

            Status = EventStatus.Completed;
            return Result.Success();
        }

        /// <summary>
        /// Add a ticket type to this event
        /// </summary>
        public Result<TicketType> AddTicketType(
            string name,
            Money price,
            int totalCapacity,
            string? description = null,
            DateTime? saleStartDate = null,
            DateTime? saleEndDate = null,
            int? minPurchaseQuantity = null,
            int? maxPurchaseQuantity = null
            )
        {
            if (Status == EventStatus.Cancelled)
                return Result.Failure<TicketType>("Cannot add ticket types to a cancelled event");

            if (Status == EventStatus.Completed)
                return Result.Failure<TicketType>("Cannot add ticket types to a completed event");

            // Validate sale dates against event dates
            if (saleStartDate.HasValue && saleStartDate.Value > StartDate)
                return Result.Failure<TicketType>("Ticket sale start date cannot be after event start date");

            if (saleEndDate.HasValue && saleEndDate.Value > StartDate)
                return Result.Failure<TicketType>("Ticket sale end date cannot be after event start date");

            // Check for duplicate ticket type names
            if (_ticketTypes.Any(t => t.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
                return Result.Failure<TicketType>($"A ticket type named '{name}' already exists for this event");

            // Create the ticket type
            var ticketTypeResult = TicketType.Create(
                Id,
                name,
                price,
                totalCapacity,
                description,
                saleStartDate,
                saleEndDate,
                minPurchaseQuantity,
                maxPurchaseQuantity
                );

            if (!ticketTypeResult.IsSuccess)
                return Result.Failure<TicketType>(ticketTypeResult.Error);

            var ticketType = ticketTypeResult.Value;

            // Add to collection
            _ticketTypes.Add(ticketType);

            return Result.Success(ticketType);
        }

        /// <summary>
        /// Remove a ticket type (only allowed if no tickets sold)
        /// </summary>
        public Result RemoveTicketType(Guid ticketTypeId)
        {
            var ticketType = _ticketTypes.FirstOrDefault(t => t.Id == ticketTypeId);

            if (ticketType == null)
                return Result.Failure("Ticket type not found");

            if (ticketType.SoldCount > 0)
                return Result.Failure("Cannot remove ticket type with sold tickets");

            if (ticketType.ReservedCount > 0)
                return Result.Failure("Cannot remove ticket type with reserved tickets");

            _ticketTypes.Remove(ticketType);

            return Result.Success();
        }

        /// <summary>
        /// Get the current live version or a specific snapshot
        /// </summary>
        public EventSnapshot? GetSnapshot(int? version = null)
        {
            if (version.HasValue)
                return _snapshots.FirstOrDefault(s => s.SnapshotVersion == version.Value);

            // Return latest snapshot
            return _snapshots.OrderByDescending(s => s.SnapshotVersion).FirstOrDefault();
        }
    }
}
