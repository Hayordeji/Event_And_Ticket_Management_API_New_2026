using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Domain.Events
{
    /// <summary>
/// Raised when an event is cancelled
/// </summary>
public sealed record EventCancelledEvent(
    Guid EventId,
    string Reason,
    DateTime OccurredAt) : DomainEvent();
}
