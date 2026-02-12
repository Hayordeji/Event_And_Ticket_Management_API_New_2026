using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Domain.Events
{
    /// <summary>
/// Raised when an event is published (made available for ticket sales)
/// </summary>
public sealed record EventPublishedEvent(
    Guid EventId,
    Guid HostId,
    string EventName,
    DateTime PublishedAt,
    DateTime OccurredAt) : DomainEvent();
}
