using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Application.Services
{
    public interface IHostBalanceService
    {
        /// <summary>
        /// Calculates a host's current balance from their ledger account.
        /// Balance = Σ Credits - Σ Debits
        /// </summary>
        Task<Result<HostBalance>> GetHostBalanceAsync(
            Guid hostId,
            CancellationToken ct = default);
    }

    public record HostBalance(
    Guid HostId,
    string AccountCode,
    decimal Balance,
    string Currency
    );
}
