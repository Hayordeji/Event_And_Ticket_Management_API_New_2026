using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Domain.Entities;
using TicketingSystem.Modules.Sales.Domain.Enums;
using TicketingSystem.Modules.Sales.Domain.Repositories;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Sales.Infrastructure.Persistence.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(SalesDbContext context) : base(context)
        {
        }

        public async Task<Order?> GetByOrderNumberAsync(
            string orderNumber,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
        }

        public async Task<Order?> GetByIdWithItemsAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        }

        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(
            Guid customerId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        //public async Task<IEnumerable<Order>> GetByEventIdAsync(
        //    Guid eventId,
        //    CancellationToken cancellationToken = default)
        //{
        //    return await _dbSet
        //        .Include(o => o.Items)
        //        .Where(o => o.Items.Select(x => x.EventId).FirstOrDefault())
        //        .OrderByDescending(o => o.CreatedAt)
        //        .ToListAsync(cancellationToken);
        //}

        public async Task<IEnumerable<Order>> GetExpiredOrdersAsync(
            DateTime expirationThreshold,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(o => o.Status == OrderStatus.Pending
                         && o.ExpiresAt < expirationThreshold)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            Guid customerId,
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(o => o.CustomerId == customerId
                            && o.Id == orderId
                            && o.Status != OrderStatus.Cancelled
                            && o.Status != OrderStatus.Expired,
                         cancellationToken);
        }
    }
}
