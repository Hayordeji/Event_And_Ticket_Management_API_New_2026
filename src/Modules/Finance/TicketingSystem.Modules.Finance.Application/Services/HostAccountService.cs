using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Entities;
using TicketingSystem.Modules.Finance.Domain.Enums;
using TicketingSystem.Modules.Finance.Infrastructure.Persistence;

namespace TicketingSystem.Modules.Finance.Application.Services
{
    /// <summary>
    /// Host account service with race condition protection using SQL MERGE
    /// </summary>
    public class HostAccountService : IHostAccountService
    {
        private readonly FinanceDbContext _context;
        private readonly ILogger<HostAccountService> _logger;

        public HostAccountService(
            FinanceDbContext context,
            ILogger<HostAccountService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> EnsureHostAccountExistsAsync(
            Guid hostId,
            string hostName,
            CancellationToken cancellationToken = default)
        {
            var accountCode = GetHostAccountCode(hostId);

            _logger.LogDebug(
                "Ensuring host account exists: {AccountCode} for host {HostName}",
                accountCode, hostName);

            // Check if account already exists (fast path - most common case)
            var exists = await _context.LedgerAccounts
                .AsNoTracking()
                .AnyAsync(a => a.AccountCode == accountCode, cancellationToken);


            if (exists)
            {
                _logger.LogDebug("Host account already exists: {AccountCode}", accountCode);
                return accountCode;
            }

            try
            {
                var account = LedgerAccount.Create(
                    accountCode: accountCode,
                    accountName: $"Host Payable - {hostName}",
                    accountType: AccountType.Liability,
                    currency: "NGN",
                    description: $"Amount owed to host: {hostName} (UserId: {hostId})");

                await _context.LedgerAccounts.AddAsync(account.Value, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Created host account: {AccountCode} for {HostName}",
                    accountCode, hostName);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique") == true
                                            || ex.InnerException?.Message.Contains("duplicate") == true)
            {
                // Race condition: another thread created the account between our check and insert
                // This is safe - the unique constraint caught it, account now exists
                _logger.LogWarning(
                    "Race condition handled: Account {AccountCode} was created by another thread. Continuing...",
                    accountCode);
            }

            return accountCode;
        }
          

        public string GetHostAccountCode(Guid hostId)
        {
            // Format: LIA-HOST-{first 8 chars of GUID}
            return $"LIA-HOST-{hostId.ToString("N")[..8].ToUpperInvariant()}";
        }

        public async Task<decimal> GetHostBalanceAsync(
            Guid hostId,
            CancellationToken cancellationToken = default)
        {
            var accountCode = GetHostAccountCode(hostId);

            var account = await _context.LedgerAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AccountCode == accountCode && !a.IsDeleted, cancellationToken);

            if (account == null)
            {
                _logger.LogWarning("Host account {AccountCode} not found", accountCode);
                return 0m;
            }

            return account.CurrentBalance.Amount;
        }
    }
}
