using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Domain.Entitites;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Fulfillment.Domain.Repositories
{
    // <summary>
    /// Repository interface for TicketDelivery entity
    /// </summary>
    public interface ITicketDeliveryRepository : IRepository<TicketDelivery>
    {
        /// <summary>
        /// Gets delivery record by order ID
        /// </summary>
        Task<TicketDelivery?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all pending deliveries for retry processing
        /// </summary>
        Task<List<TicketDelivery>> GetPendingDeliveriesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets failed deliveries that can be retried
        /// </summary>
        Task<List<TicketDelivery>> GetFailedDeliveriesForRetryAsync(CancellationToken cancellationToken = default);
    }
}
