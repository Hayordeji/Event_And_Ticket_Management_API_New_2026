using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Access.Domain.Entities;
using TicketingSystem.Modules.Access.Domain.Enums;
using TicketingSystem.Modules.Access.Domain.Repositories;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Access.Infrastructure.Persistence.Repositories
{
    public class ScanLogRepository : Repository<ScanLog>, IScanLogRepository
    {
        public ScanLogRepository(AccessDbContext context) : base(context) { }

        public async Task<List<ScanLog>> GetByEventIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<ScanLog>()
                .Where(s => s.EventId == eventId)
                .OrderByDescending(s => s.ScannedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ScanLog>> GetByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<ScanLog>()
                .Where(s => s.TicketId == ticketId)
                .OrderByDescending(s => s.ScannedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasBeenScannedSuccessfullyAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<ScanLog>()
                .AnyAsync(s => s.TicketId == ticketId && s.Result == ScanResult.Allowed, cancellationToken);
        }

        public async Task<int> GetAllowedCountAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<ScanLog>()
                .CountAsync(s => s.EventId == eventId && s.Result == ScanResult.Allowed, cancellationToken);
        }

        public async Task<int> GetDeniedCountAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<ScanLog>()
                .CountAsync(s => s.EventId == eventId && s.Result == ScanResult.Denied, cancellationToken);
        }
    }
}
