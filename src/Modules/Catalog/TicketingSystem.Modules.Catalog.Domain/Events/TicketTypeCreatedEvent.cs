using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Domain.Events
{
    /// <summary>
/// Raised when a new ticket type is created for an event
/// </summary>
public sealed record TicketTypeCreatedEvent(
    Guid TicketTypeId,
    Guid EventId,
    string TicketTypeName,
    decimal Price,
    int TotalCapacity,
    DateTime OccurredAt) : DomainEvent();
}
