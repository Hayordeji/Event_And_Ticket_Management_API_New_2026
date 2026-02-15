using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Domain.Entitites;
using TicketingSystem.Modules.Fulfillment.Domain.Repositories;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence.Repositories
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(FulfillmentDbContext context) : base(context)
        {
        }

        public async Task<Ticket?> GetByTicketNumberAsync(
            string ticketNumber,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<Ticket>()
                .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber, cancellationToken);
        }

        public async Task<List<Ticket>> GetByOrderIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<Ticket>()
                .Where(t => t.OrderId == orderId)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Ticket>> GetByOrderNumberAsync(
            string orderNumber,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<Ticket>()
                .Where(t => t.OrderNumber == orderNumber)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Ticket>> GetByCustomerIdAsync(
            Guid customerId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<Ticket>()
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.EventStartDate)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Ticket>> GetByEventIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<Ticket>()
                .Where(t => t.EventId == eventId)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Ticket?> GetByQrCodeDataAsync(
            string qrCodeData,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<Ticket>()
                .FirstOrDefaultAsync(t => t.QrCodeData == qrCodeData, cancellationToken);
        }
    }
}
