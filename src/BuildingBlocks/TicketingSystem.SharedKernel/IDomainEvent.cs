using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel
{
/// <summary>
/// Marker interface for domain events
/// Domain events are things that have happened in the domain
/// </summary>
    public interface IDomainEvent
    {
        Guid EventId { get; }
        DateTime OccurredAt { get; }
    }
}
