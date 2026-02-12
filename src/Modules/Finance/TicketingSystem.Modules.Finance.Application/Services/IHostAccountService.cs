using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Finance.Application.Services
{
    /// <summary>
    /// Service for managing host-specific ledger accounts
    /// Handles creation and retrieval with race condition protection
    /// </summary>
    public interface IHostAccountService
    {
        /// <summary>
        /// Ensures host account exists (creates if not exists) - Idempotent
        /// </summary>
        Task<string> EnsureHostAccountExistsAsync(
            Guid hostId,
            string hostName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets host's account code
        /// </summary>
        string GetHostAccountCode(Guid hostId);

        /// <summary>
        /// Gets host's current balance
        /// </summary>
        Task<decimal> GetHostBalanceAsync(
            Guid hostId,
            CancellationToken cancellationToken = default);
    }
}
