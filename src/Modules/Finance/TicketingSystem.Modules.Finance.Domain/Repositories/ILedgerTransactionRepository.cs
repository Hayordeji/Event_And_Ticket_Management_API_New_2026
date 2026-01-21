using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Entities;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Domain.Repositories
{
    public interface ILedgerTransactionRepository : IRepository<LedgerTransaction>
    {
        /// <summary>
        /// Get transaction by reference (e.g., OrderId)
        /// </summary>
        Task<LedgerTransaction?> GetByReferenceAsync(string referenceType, Guid referenceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get transaction with all entries included
        /// </summary>
        Task<LedgerTransaction?> GetByIdWithEntriesAsync(Guid transactionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all transactions for a specific reference type
        /// </summary>
        Task<IReadOnlyList<LedgerTransaction>> GetByReferenceTypeAsync(string referenceType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if transaction already exists for a reference
        /// </summary>
        Task<bool> TransactionExistsForReferenceAsync(string referenceType, Guid referenceId, CancellationToken cancellationToken = default);
    }
}
