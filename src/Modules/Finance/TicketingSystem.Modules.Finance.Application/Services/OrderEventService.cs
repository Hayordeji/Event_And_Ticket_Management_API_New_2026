using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Application.Services
{
    public class OrderEventService : IOrderEventService
    {
        private readonly SalesDbContext _salesContext;

        public OrderEventService(SalesDbContext salesContext)
        {
            _salesContext = salesContext;
        }

        public async Task<Result<Guid>> GetEventIdByOrderIdAsync(
            Guid orderId,
            CancellationToken ct = default)
        {
            var eventId = await _salesContext.Orders
                .Where(o => o.Id == orderId)
                .Select(o => (Guid?)o.EventId)
                .FirstOrDefaultAsync(ct);

            if (eventId is null)
                return Result.Failure<Guid>(
                    $"Order {orderId} not found. Cannot resolve EventId.");

            return Result.Success(eventId.Value);
        }
    }
}
