using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Entities;
using TicketingSystem.Modules.Finance.Domain.Repositories;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Finance.Infrastructure.Persistence.Repositories
{
    public class LedgerTransactionRepository : Repository<LedgerTransaction>, ILedgerTransactionRepository
    {
        private readonly FinanceDbContext _context;

        public LedgerTransactionRepository(FinanceDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<LedgerTransaction?> GetByReferenceAsync(string referenceType, Guid referenceId, CancellationToken cancellationToken = default)
        {
            return await _context.LedgerTransactions
                .Include(t => t.Entries)
                    .ThenInclude(e => e.Account)
                .FirstOrDefaultAsync(t => t.ReferenceType == referenceType && t.ReferenceId == referenceId, cancellationToken);
        }

        public async Task<LedgerTransaction?> GetByIdWithEntriesAsync(Guid transactionId, CancellationToken cancellationToken = default)
        {
            return await _context.LedgerTransactions
                .Include(t => t.Entries)
                    .ThenInclude(e => e.Account)
                .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);
        }

        public async Task<IReadOnlyList<LedgerTransaction>> GetByReferenceTypeAsync(string referenceType, CancellationToken cancellationToken = default)
        {
            return await _context.LedgerTransactions
                .Include(t => t.Entries)
                .Where(t => t.ReferenceType == referenceType)
                .OrderByDescending(t => t.OccurredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> TransactionExistsForReferenceAsync(string referenceType, Guid referenceId, CancellationToken cancellationToken = default)
        {
            return await _context.LedgerTransactions
                .AnyAsync(t => t.ReferenceType == referenceType && t.ReferenceId == referenceId, cancellationToken);
        }
    }
}
