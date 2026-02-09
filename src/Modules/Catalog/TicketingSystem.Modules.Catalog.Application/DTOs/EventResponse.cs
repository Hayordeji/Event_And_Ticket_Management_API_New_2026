using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Catalog.Application.DTOs
{
    public record EventResponse(
    Guid Id,
    Guid HostId,
    string Name,
    string Description,
    VenueDto Venue,
    DateTime StartDate,
    DateTime? EndDate,
    string? ImageUrl,
    string Status,
    DateTime? PublishedAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    bool HasSnapshot,
    int CurrentSnapshotVersion,
    DateTime CreatedAt);
}
