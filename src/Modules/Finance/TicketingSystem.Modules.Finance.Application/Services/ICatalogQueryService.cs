using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Finance.Application.Services
{
    /// <summary>
    /// Service for querying event data from Catalog module
    /// Used to get HostId for creating host-specific ledger accounts
    /// </summary>
    public interface ICatalogQueryService
    {
        Task<EventDataDto?> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default);

    }
    public sealed record EventDataDto(
    Guid EventId,
    Guid HostId,
    string HostName,
    string EventName);
}
