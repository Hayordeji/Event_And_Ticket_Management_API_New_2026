using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Domain.Events
{
    /// <summary>
/// Raised when an event is updated
/// </summary>
    public record EventUpdatedEvent(
    Guid HostEventId,
    bool SnapshotCreated,
    int? SnapshotVersion) : DomainEvent();
}
    