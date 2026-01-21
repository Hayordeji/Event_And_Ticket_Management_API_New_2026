using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Entities;
using TicketingSystem.Modules.Finance.Infrastructure.Persistence;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Finance.Domain.Repositories
{
    public class LedgerAccountRepository : Repository<LedgerAccount>, ILedgerAccountRepository
    {
        private readonly FinanceDbContext _context;

        public LedgerAccountRepository(FinanceDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<LedgerAccount?> GetByAccountCodeAsync(string accountCode, CancellationToken cancellationToken = default)
        {
            return await _context.LedgerAccounts
                .FirstOrDefaultAsync(a => a.AccountCode == accountCode.ToUpperInvariant(), cancellationToken);
        }

        public async Task<LedgerAccount?> GetByAccountNameAsync(string accountName, CancellationToken cancellationToken = default)
        {
            return await _context.LedgerAccounts
                .FirstOrDefaultAsync(a => a.AccountName == accountName, cancellationToken);
        }

        public async Task<bool> AccountCodeExistsAsync(string accountCode, CancellationToken cancellationToken = default)
        {
            return await _context.LedgerAccounts
                .AnyAsync(a => a.AccountCode == accountCode.ToUpperInvariant(), cancellationToken);
        }

        public async Task<IReadOnlyList<LedgerAccount>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.LedgerAccounts
                .Where(a => a.IsActive)
                .OrderBy(a => a.AccountCode)
                .ToListAsync(cancellationToken);
        }
    }
}
