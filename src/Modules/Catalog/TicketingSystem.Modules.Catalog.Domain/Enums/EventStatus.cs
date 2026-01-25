using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Catalog.Domain.Enums
{
    public enum EventStatus
    {
        // <summary>
        /// Event is being created, not yet published
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Event is published and tickets can be sold
        /// </summary>
        Published = 1,

        /// <summary>
        /// Event has ended
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Event was cancelled
        /// </summary>
        Cancelled = 3,

        /// <summary>
        /// Event is postponed (can be rescheduled)
        /// </summary>
        Postponed = 4
    }
}
