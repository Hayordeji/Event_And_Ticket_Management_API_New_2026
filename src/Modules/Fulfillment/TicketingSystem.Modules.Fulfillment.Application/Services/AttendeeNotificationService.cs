using Microsoft.EntityFrameworkCore;
using TicketingSystem.Modules.Fulfillment.Application.Services;
using TicketingSystem.Modules.Fulfillment.Domain.Enums;
using TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence;

namespace TicketingSystem.Modules.Catalog.Infrastructure.Services
{
    /// <summary>
    /// Resolves attendee information from Fulfillment module
    /// Queries tickets where EventId matches and Status is Valid
    /// </summary>
    public class AttendeeNotificationService : IAttendeeNotificationService
    {
        private readonly FulfillmentDbContext _fulfillmentContext;

        public AttendeeNotificationService(FulfillmentDbContext fulfillmentContext)
        {
            _fulfillmentContext = fulfillmentContext;
        }

        public async Task<List<AttendeeDto>> GetAttendeesByEventIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            var attendees = await _fulfillmentContext.Tickets
                .Where(t => t.EventId == eventId && t.Status == TicketStatus.Valid)
                .GroupBy(t => t.CustomerEmail)
                .Select(g => new AttendeeDto(
                    g.Key,
                    $"{g.First().CustomerFirstName} {g.First().CustomerLastName}",
                    g.First().OrderNumber))
                .ToListAsync(cancellationToken);

            return attendees;
        }
    }
}