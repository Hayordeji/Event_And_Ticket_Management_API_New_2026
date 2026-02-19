using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Application.Services
{
    public class EventHostService : IEventHostService
    {
        private readonly CatalogDbContext _catalogContext;

        public EventHostService(CatalogDbContext catalogContext)
        {
            _catalogContext = catalogContext;
        }

        public async Task<Result<Guid>> GetHostIdAsync(
            Guid eventId,
            CancellationToken ct = default)
        {
            var hostId = await _catalogContext.Events
                .Where(e => e.Id == eventId)
                .Select(e => (Guid?)e.HostId)
                .FirstOrDefaultAsync(ct);

            if (hostId is null)
                return Result.Failure<Guid>(
                    $"Event {eventId} not found. Cannot resolve HostId.");

            return Result.Success(hostId.Value);
        }
    }
}
