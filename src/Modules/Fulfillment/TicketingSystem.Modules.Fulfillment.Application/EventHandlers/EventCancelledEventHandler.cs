using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Events;
using TicketingSystem.Modules.Fulfillment.Domain.Enums;
using TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence;

namespace TicketingSystem.Modules.Fulfillment.Application.EventHandlers
{
    public class EventCancelledEventHandler : INotificationHandler<EventCancelledEvent>
    {
        private readonly FulfillmentDbContext _context;

        public EventCancelledEventHandler(FulfillmentDbContext context) => _context = context;

        public async Task Handle(EventCancelledEvent notification, CancellationToken ct)
        {
            var tickets = await _context.Tickets
                .Where(t => t.EventId == notification.EventId
                    && t.Status == TicketStatus.Valid)
                .ToListAsync(ct);

            if (!tickets.Any()) return;

            foreach (var ticket in tickets)
                ticket.Cancel(notification.Reason); // raises status change inside aggregate

            await _context.SaveChangesAsync(ct);
        }
    }
}
