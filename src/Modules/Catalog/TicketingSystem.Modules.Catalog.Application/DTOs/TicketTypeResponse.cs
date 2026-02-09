using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Catalog.Application.DTOs
{
    public record TicketTypeResponse(
    Guid Id,
    Guid EventId,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int TotalCapacity,
    int SoldCount,
    int ReservedCount,
    int AvailableCount,
    bool IsSoldOut,
    DateTime? SaleStartDate,
    DateTime? SaleEndDate,
    int? MinPurchaseQuantity,
    int? MaxPurchaseQuantity,
    bool IsActive);
}
