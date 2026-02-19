using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Enums;
using TicketingSystem.Modules.Finance.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Application.Services
{
    public class HostBalanceService : IHostBalanceService
    {
        private readonly FinanceDbContext _context;

        public HostBalanceService(FinanceDbContext context)
        {
            _context = context;
        }

        public async Task<Result<HostBalance>> GetHostBalanceAsync(
            Guid hostId,
            CancellationToken ct = default)
        {
            var accountCode = GetHostAccountCode(hostId);

            var account = await _context.LedgerAccounts
                .FirstOrDefaultAsync(a => a.AccountCode == accountCode, ct);

            if (account == null)
                return Result.Failure<HostBalance>(
                    $"Host account {accountCode} not found. Has the host published any events?");

            // Calculate balance: Σ Credits - Σ Debits
            var balance = await _context.LedgerEntries
                .Where(e => e.AccountId == account.Id)
                .SumAsync(e =>
                    e.EntryType == EntryType.Credit ? e.Amount.Amount : -e.Amount.Amount,
                    ct);

            return Result.Success(new HostBalance(
                HostId: hostId,
                AccountCode: accountCode,
                Balance: balance,
                Currency: "NGN" // TODO: Make currency configurable per host
            ));
        }


        public string GetHostAccountCode(Guid hostId)
        {
            // Format: LIA-HOST-{first 8 chars of GUID}
            return $"LIA-HOST-{hostId.ToString("N")[..8].ToUpperInvariant()}";
        }
    }
}
