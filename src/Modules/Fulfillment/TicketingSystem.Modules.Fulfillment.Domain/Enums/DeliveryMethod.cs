using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Fulfillment.Domain.Enums
{
    /// <summary>
    /// Method used to deliver tickets to customer
    /// </summary>
    public enum DeliveryMethod
    {
        /// <summary>
        /// Tickets delivered via email
        /// </summary>
        Email,

        /// <summary>
        /// Tickets available for download (no email)
        /// </summary>
        Download,

        /// <summary>
        /// Tickets sent via SMS (future)
        /// </summary>
        Sms,

        /// <summary>
        /// Tickets available in mobile app (future)
        /// </summary>
        MobileApp
    }

}
