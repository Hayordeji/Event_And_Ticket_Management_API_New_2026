using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Fulfillment.Application.Services
{
    /// <summary>
    /// Service for fetching order data from Sales module
    /// Used by event handlers to get complete order information
    /// </summary>
    public interface IOrderDataService
    {
        Task<OrderDataDto?> GetOrderDataAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<OrderDataDto?> GetOrderDataByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// DTO containing complete order data needed for ticket generation
    /// </summary>
    public sealed record OrderDataDto(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        string CustomerFirstName,
        string CustomerLastName,
        Guid EventId,
        string EventName,
        string EventDescription,
        DateTime EventStartDate,
        DateTime EventEndDate,
        string VenueName,
        string VenueAddress,
        string VenueCity,
        string Currency,
        List<OrderItemDataDto> Items);

    public sealed record OrderItemDataDto(
        Guid TicketTypeId,
        string TicketTypeName,
        string TicketTypeDescription,
        int Quantity,
        decimal UnitPrice);
}
