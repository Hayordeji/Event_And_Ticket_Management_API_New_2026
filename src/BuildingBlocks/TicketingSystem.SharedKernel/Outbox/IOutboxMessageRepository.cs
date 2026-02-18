using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Outbox
{
    public interface IOutboxMessageRepository
    {
        /// <summary>
        /// Retrieve messages ready for processing:
        /// - Status = Pending
        /// - RetryAt is null OR RetryAt &lt;= now
        /// </summary>
        Task<List<OutboxMessage>> GetPendingMessagesAsync(
            int batchSize = 20,
            CancellationToken ct = default);

        /// <summary>
        /// Update a message after processing attempt (success or failure).
        /// </summary>
        Task UpdateAsync(OutboxMessage message, CancellationToken ct = default);
    }
}
