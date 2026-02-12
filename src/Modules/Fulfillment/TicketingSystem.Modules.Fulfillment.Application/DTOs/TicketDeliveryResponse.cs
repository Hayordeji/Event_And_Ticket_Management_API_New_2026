using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Fulfillment.Application.DTOs
{
    /// <summary>
    /// Response DTO for ticket delivery status
    /// </summary>
    public record TicketDeliveryResponse(
        Guid DeliveryId,
        Guid OrderId,
        string OrderNumber,
        string RecipientEmail,
        string Status,
        DateTime? SentAt,
        DateTime? DeliveredAt,
        int AttemptCount,
        string? FailureReason);
   
}
