using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Domain.Enums;

namespace TicketingSystem.Modules.Sales.Application.DTOs
{
    public record ProcessPaymentRequest(
    string OrderNumber,
    PaymentMethod PaymentMethod,
    string PaymentReference,
    string GatewayResponse
    );
}
