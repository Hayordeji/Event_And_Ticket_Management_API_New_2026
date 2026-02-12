using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Domain.Entitites;
using TicketingSystem.Modules.Fulfillment.Domain.Enums;
using TicketingSystem.Modules.Fulfillment.Domain.Repositories;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence.Repositories
{
    public class TicketDeliveryRepository : Repository<TicketDelivery>, ITicketDeliveryRepository
    {
        public TicketDeliveryRepository(FulfillmentDbContext context) : base(context)
        {
        }

        public async Task<TicketDelivery?> GetByOrderIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<TicketDelivery>()
                .FirstOrDefaultAsync(td => td.OrderId == orderId, cancellationToken);
        }

        public async Task<List<TicketDelivery>> GetPendingDeliveriesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<TicketDelivery>()
                .Where(td => td.Status == DeliveryStatus.Pending)
                .OrderBy(td => td.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<TicketDelivery>> GetFailedDeliveriesForRetryAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<TicketDelivery>()
                .Where(td => td.Status == DeliveryStatus.Failed && td.AttemptCount < 3)
                .OrderBy(td => td.FailedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
