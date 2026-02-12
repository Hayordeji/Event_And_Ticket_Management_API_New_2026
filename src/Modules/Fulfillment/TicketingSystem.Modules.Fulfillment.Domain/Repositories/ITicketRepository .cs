using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Domain.Entitites;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Fulfillment.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Ticket aggregate
    /// </summary>
    public interface ITicketRepository : IRepository<Ticket>
    {
        /// <summary>
        /// Gets a ticket by its unique ticket number
        /// </summary>
        Task<Ticket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all tickets for a specific order
        /// </summary>
        Task<List<Ticket>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all tickets for a specific order number
        /// </summary>
        Task<List<Ticket>> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all tickets for a specific customer
        /// </summary>
        Task<List<Ticket>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all tickets for a specific event
        /// </summary>
        Task<List<Ticket>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets ticket by QR code data (for scanning)
        /// </summary>
        Task<Ticket?> GetByQrCodeDataAsync(string qrCodeData, CancellationToken cancellationToken = default);

    }
}
