using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Fulfillment.Application.DTOs
{
    ///<summary>
    /// Response DTO containing ticket details for API responses
    /// </summary>
    public record TicketResponse(
        Guid TicketId,
        string TicketNumber,
        string OrderNumber,
        string EventName,
        DateTime EventStartDate,
        DateTime EventEndDate,
        string VenueName,
        string VenueAddress,
        string VenueCity,
        string TicketTypeName,
        string CustomerFirstName,
        string CustomerLastName,
        decimal PricePaid,
        string Currency,
        string Status,
        string QrCodeData,
        string Barcode,
        DateTime? UsedAt,
        DateTime CreatedAt);

}
