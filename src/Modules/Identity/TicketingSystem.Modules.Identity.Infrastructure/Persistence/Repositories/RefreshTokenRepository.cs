using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Domain.Entities;
using TicketingSystem.Modules.Identity.Domain.Repositories;

namespace TicketingSystem.Modules.Identity.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IdentityAppDbContext _context;

        public RefreshTokenRepository(IdentityAppDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(
            string token,
            CancellationToken ct = default)
            => await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == token, ct);

        public async Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(
            Guid userId,
            CancellationToken ct = default)
            => await _context.RefreshTokens
                .Where(r => r.UserId == userId
                         && r.RevokedAt == null
                         && r.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(ct);

        public async Task AddAsync(
            RefreshToken token,
            CancellationToken ct = default)
            => await _context.RefreshTokens.AddAsync(token, ct);
    }
}
