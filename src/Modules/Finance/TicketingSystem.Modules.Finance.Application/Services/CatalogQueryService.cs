using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Infrastructure.Persistence;
using TicketingSystem.Modules.Identity.Infrastructure.Persistence;

namespace TicketingSystem.Modules.Finance.Application.Services
{
    public class CatalogQueryService : ICatalogQueryService
    {
        private readonly CatalogDbContext _catalogContext;
        private readonly IdentityDbContext _identityContext;
        private readonly ILogger<CatalogQueryService> _logger;

        public CatalogQueryService(
            CatalogDbContext catalogContext,
            IdentityDbContext identityContext,
            ILogger<CatalogQueryService> logger)
        {
            _catalogContext = catalogContext;
            _identityContext = identityContext;
            _logger = logger;
        }

        public async Task<EventDataDto?> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Fetching event data for EventId={EventId}", eventId);

            var eventEntity = await _catalogContext.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == eventId && !e.IsDeleted, cancellationToken);

            if (eventEntity == null)
            {
                _logger.LogWarning("Event {EventId} not found", eventId);
                return null;
            }

            // Get host details
            var host = await _identityContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == eventEntity.HostId && !u.IsDeleted, cancellationToken);

            var hostName = host != null
                ? $"{host.FirstName} {host.LastName}"
                : $"Host-{eventEntity.HostId.ToString()[..8]}";

            _logger.LogDebug(
                "Event data fetched: EventName={EventName}, HostId={HostId}, HostName={HostName}",
                eventEntity.Name, eventEntity.HostId, hostName);

            return new EventDataDto(
                EventId: eventEntity.Id,
                HostId: eventEntity.HostId,
                HostName: hostName,
                EventName: eventEntity.Name);
        }
    }
}
