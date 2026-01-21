using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Domain.Events
{
    ///<summary>
/// Raised when a user logs in
/// </summary>
    public record UserLoggedInEvent(Guid UserId,
    string Email,
    string DeviceFingerprintHash,
    DateTime LoggedInAt) : DomainEvent;
    
}
