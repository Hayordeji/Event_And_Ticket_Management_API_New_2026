using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Infrastructure.Persistence;
using TicketingSystem.Modules.Sales.Domain.Events;

namespace TicketingSystem.Modules.Finance.Application.EventHandlers
{
    public class OrderExpiredEventHandler : INotificationHandler<OrderExpiredEvent>
    {
        private readonly CatalogDbContext _context;
        private readonly ILogger<OrderExpiredEventHandler> _logger;

        public OrderExpiredEventHandler(
            CatalogDbContext context,
            ILogger<OrderExpiredEventHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Handle(
            OrderExpiredEvent notification,
            CancellationToken cancellationToken)
        {
            var quantityByTicketType = notification.Items
                .GroupBy(i => i.TicketTypeId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            var ticketTypeIds = quantityByTicketType.Keys.ToList();

            var ticketTypes = await _context.TicketTypes
                .Where(tt => ticketTypeIds.Contains(tt.Id))
                .ToListAsync(cancellationToken);

            foreach (var ticketType in ticketTypes)
            {
                var quantity = quantityByTicketType[ticketType.Id];
                var result = ticketType.ReleaseReservation(quantity);

                if (result.IsFailure)
                {
                    // Log but continue — partial release is better than blocking everything
                    _logger.LogWarning(
                        "Failed to release {Quantity} tickets for TicketType {TicketTypeId} " +
                        "on expired order {OrderNumber}: {Error}",
                        quantity,
                        ticketType.Id,
                        notification.OrderNumber,
                        result.Error);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
