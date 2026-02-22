using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Events;
using TicketingSystem.Modules.Sales.Application.Commands;
using TicketingSystem.Modules.Sales.Domain.Enums;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;

namespace TicketingSystem.Modules.Sales.Application.EventHandlers
{
    public class EventCancelledEventHandler : INotificationHandler<EventCancelledEvent>
    {
        private readonly SalesDbContext _context;
        private readonly IMediator _mediator;
        private readonly ILogger<EventCancelledEventHandler> _logger;

        public EventCancelledEventHandler(
            SalesDbContext context,
            IMediator mediator,
            ILogger<EventCancelledEventHandler> logger)
        {
            _context = context;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(EventCancelledEvent notification, CancellationToken ct)
        {
            var activeOrders = await _context.Orders
                .Where(o => o.EventId == notification.EventId
                    && o.Status == OrderStatus.Paid)
                .ToListAsync(ct);

            if (!activeOrders.Any()) return;

            foreach (var order in activeOrders)
            {
                var result = await _mediator.Send(
                    new RequestRefundCommand(order.OrderNumber, notification.Reason), ct);

                if (result.IsFailure)
                    _logger.LogError(
                        "Failed to refund order {OrderNumber}: {Error}",
                        order.OrderNumber, result.Error);
            }
        }
    }
}
