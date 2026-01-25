using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Catalog.Application.DTOs
{
   public record AddTicketTypeRequest(
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int TotalCapacity,
    DateTime? SaleStartDate,
    DateTime? SaleEndDate,
    int? MinPurchaseQuantity,
    int? MaxPurchaseQuantity);
}
