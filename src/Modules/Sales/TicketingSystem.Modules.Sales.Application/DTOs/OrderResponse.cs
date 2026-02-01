using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Domain.Enums;

namespace TicketingSystem.Modules.Sales.Application.DTOs
{
    public record OrderResponse(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    Guid EventId,
    OrderStatus Status,
    decimal SubTotal,
    decimal ServiceFee,
    decimal GrandTotal,
    string Currency,
    DateTime CreatedAt,
    DateTime? PaidAt,
    DateTime? ExpiresAt,
    List<OrderItemResponse> Items,
    List<PaymentResponse> Payments
);

    public record OrderItemResponse(
        Guid Id,
        Guid TicketTypeId,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice
    );

    public record PaymentResponse(
        Guid Id,
        decimal Amount,
        string Currency,
        PaymentMethod Method,
        PaymentStatus Status,
        string? PaymentReference,
        DateTime? PaidAt
    );
}
