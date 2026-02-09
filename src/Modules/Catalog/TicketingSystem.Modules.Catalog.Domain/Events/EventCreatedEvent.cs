using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Domain.Events
{
    /// <summary>
/// Raised when a new event is created
/// </summary>
public sealed record EventCreatedEvent(
    Guid EventId,
    string EventName,
    Guid HostId,
    DateTime EventDate,
    DateTime OccurredAt) : DomainEvent();
}
