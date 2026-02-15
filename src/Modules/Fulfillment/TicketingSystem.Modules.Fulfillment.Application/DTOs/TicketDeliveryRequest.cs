using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Fulfillment.Application.DTOs
{
    /// <summary>
    /// Request DTO for sending tickets to customer
    /// </summary>
    public record TicketDeliveryRequest(
        Guid OrderId,
        string RecipientEmail,
        List<Guid> TicketIds);
}
