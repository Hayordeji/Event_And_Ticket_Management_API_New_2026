using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Access.Domain.Enums
{
    public enum DenialReason
    {
        AlreadyUsed,
        TicketCancelled,
        TicketExpired,
        InvalidTicket,
        EventMismatch
    }
}
