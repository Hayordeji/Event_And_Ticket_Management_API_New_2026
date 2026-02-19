using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Outbox
{
    public enum OutboxMessageStatus
    {
        /// <summary>
        /// Message is waiting to be processed or is scheduled for retry.
        /// </summary>
        Pending,

        /// <summary>
        /// Message was successfully processed and handlers completed.
        /// </summary>
        Processed,

        /// <summary>
        /// Message failed after max retries and requires manual intervention.
        /// </summary>
        DeadLettered
    }
}
