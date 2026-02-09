using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Domain.Entities;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Domain.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

        Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

        //Task<IEnumerable<Order>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Order>> GetExpiredOrdersAsync(DateTime expirationThreshold, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Guid customerId, Guid eventId, CancellationToken cancellationToken = default);
    }
}
