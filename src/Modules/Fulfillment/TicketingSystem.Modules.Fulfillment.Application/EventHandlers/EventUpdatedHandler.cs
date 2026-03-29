using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketingSystem.Modules.Fulfillment.Application.Services;
using TicketingSystem.Modules.Catalog.Domain.Events;
using TicketingSystem.Modules.Catalog.Infrastructure.Persistence;
using TicketingSystem.SharedKernel.Services;

namespace TicketingSystem.Modules.Fulfillment.Application.EventHandlers
{
    /// <summary>
    /// Handles EventUpdatedEvent by sending update notifications to all attendees
    /// Event raised when a host or admin updates event details
    /// </summary>
    public class EventUpdatedEventHandler : INotificationHandler<EventUpdatedEvent>
    {
        private readonly IAttendeeNotificationService _attendeeNotificationService;
        private readonly IEmailService _emailService;
        private readonly CatalogDbContext _catalogContext;
        private readonly ILogger<EventUpdatedEventHandler> _logger;

        public EventUpdatedEventHandler(
            IAttendeeNotificationService attendeeNotificationService,
            IEmailService emailService,
            CatalogDbContext catalogContext,
            ILogger<EventUpdatedEventHandler> logger)
        {
            _attendeeNotificationService = attendeeNotificationService;
            _emailService = emailService;
            _catalogContext = catalogContext;
            _logger = logger;
        }

        public async Task Handle(EventUpdatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Processing event update notifications for EventId={EventId}, SnapshotCreated={SnapshotCreated}",
                    notification.HostEventId, notification.SnapshotCreated);

                // Step 1: Fetch attendees for the updated event
                var attendees = await _attendeeNotificationService.GetAttendeesByEventIdAsync(
                    notification.HostEventId,
                    cancellationToken);

                // Idempotency check: No attendees to notify
                if (!attendees.Any())
                {
                    _logger.LogInformation(
                        "No attendees found for updated EventId={EventId}. Skipping notifications.",
                        notification.HostEventId);
                    return;
                }

                // Step 2: Fetch event name from catalog
                var eventEntity = await _catalogContext.Events
                    .FirstOrDefaultAsync(e => e.Id == notification.HostEventId, cancellationToken);

                if (eventEntity == null)
                {
                    _logger.LogWarning(
                        "Event not found for update notification. EventId={EventId}",
                        notification.HostEventId);
                    return;
                }

                // Step 3: Send update email to each attendee
                foreach (var attendee in attendees)
                {
                    try
                    {
                        await _emailService.SendEventUpdatedEmailAsync(
                            recipientEmail: attendee.Email,
                            recipientName: attendee.Name,
                            eventName: eventEntity.Name,
                            cancellationToken: cancellationToken);

                        _logger.LogInformation(
                            "Event update email sent to {Email} for EventId={EventId}",
                            attendee.Email, notification.HostEventId);
                    }
                    catch (Exception ex)
                    {
                        // Log failure per attendee, never throw (Outbox retry behavior)
                        _logger.LogError(
                            ex,
                            "Failed to send event update email to {Email} for EventId={EventId}",
                            attendee.Email, notification.HostEventId);
                    }
                }

                _logger.LogInformation(
                    "Event update notifications completed for EventId={EventId}. NotifiedCount={Count}",
                    notification.HostEventId, attendees.Count);
            }
            catch (Exception ex)
            {
                // Log critical errors but don't throw (Outbox pattern requires idempotent handlers)
                _logger.LogError(
                    ex,
                    "Critical error processing event update notifications for EventId={EventId}",
                    notification.HostEventId);
            }
        }
    }
}