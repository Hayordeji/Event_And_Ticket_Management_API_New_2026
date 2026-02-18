using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel.Outbox;

namespace TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence.Repositories
{
    public class OutboxMessageRepository : IOutboxMessageRepository
    {
        private readonly FulfillmentDbContext _context;

        public OutboxMessageRepository(FulfillmentDbContext context)
        {
            _context = context;
        }

        public async Task<List<OutboxMessage>> GetPendingMessagesAsync(
            int batchSize = 20,
            CancellationToken ct = default)
        {
            return await _context.OutboxMessages
                .Where(m => m.Status == OutboxMessageStatus.Pending
                         && (m.RetryAt == null || m.RetryAt <= DateTime.UtcNow))
                .OrderBy(m => m.CreatedAt)
                .Take(batchSize)
                .ToListAsync(ct);
        }

        public async Task UpdateAsync(OutboxMessage message, CancellationToken ct = default)
        {
            _context.OutboxMessages.Update(message);
            await _context.SaveChangesAsync(ct);
        }
    }
}
