using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Fulfillment.Domain.Enums
{
    /// <summary>
    /// Represents the lifecycle status of a ticket
    /// </summary>
    public enum TicketStatus
    {
        /// <summary>
        /// Ticket is valid and can be used
        /// </summary>
        Valid,

        /// <summary>
        /// Ticket has been used (scanned at entrance)
        /// </summary>
        Used,

        /// <summary>
        /// Ticket has been cancelled (refund scenario)
        /// </summary>
        Cancelled,

        /// <summary>
        /// Ticket has expired (event ended)
        /// </summary>
        Expired
    }
}
