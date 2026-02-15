using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Fulfillment.Domain.Enums
{
    /// <summary>
    /// Represents the status of ticket delivery to customer
    /// </summary>
    public enum DeliveryStatus
    {
        /// <summary>
        /// Delivery is pending (not yet attempted)
        /// </summary>
        Pending,

        /// <summary>
        /// Delivery is in progress (email being sent)
        /// </summary>
        Sending,

        /// <summary>
        /// Email has been sent (accepted by provider)
        /// </summary>
        Sent,

        /// <summary>
        /// Email has been delivered to recipient
        /// </summary>
        Delivered,

        /// <summary>
        /// Delivery failed (will retry if attempts < 3)
        /// </summary>
        Failed
    }
}
