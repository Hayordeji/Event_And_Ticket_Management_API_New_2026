using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Domain.Events
{
    /// <summary>
/// Raised when an event is updated
/// </summary>
public sealed record EventUpdatedEvent(
    Guid EventId,
    bool SnapshotCreated,
    int? SnapshotVersion,
    DateTime OccurredAt) : DomainEvent();
}
