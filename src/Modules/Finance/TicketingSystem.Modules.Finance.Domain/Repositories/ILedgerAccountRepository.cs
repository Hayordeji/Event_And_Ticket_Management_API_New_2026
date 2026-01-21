using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Entities;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Domain.Repositories
{
    public interface ILedgerAccountRepository : IRepository<LedgerAccount>
    {
        /// <summary>
        /// Get account by account code
        /// </summary>
        Task<LedgerAccount?> GetByAccountCodeAsync(string accountCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get account by account name
        /// </summary>
        Task<LedgerAccount?> GetByAccountNameAsync(string accountName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if account code already exists
        /// </summary>
        Task<bool> AccountCodeExistsAsync(string accountCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all active accounts
        /// </summary>
        Task<IReadOnlyList<LedgerAccount>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
    }
}
