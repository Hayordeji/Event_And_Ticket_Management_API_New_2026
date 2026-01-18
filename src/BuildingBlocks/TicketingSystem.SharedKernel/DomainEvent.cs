using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel
{
    ///<summary>
    /// Base implementation for domain events
    /// </summary>
    public abstract record  DomainEvent:IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
}
