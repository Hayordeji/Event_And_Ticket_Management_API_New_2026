using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Domain.Events
{
    ///<summary>
/// Raised when a new user registers
/// </summary>
    public record UserRegisteredEvent(Guid UserId,
    string Email,
    DateTime RegisteredAt) : DomainEvent;
    
}
