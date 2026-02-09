using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Sales.Application.DTOs
{
    public record PaymentVerificationResponse(
    bool IsSuccessful,
    string PaymentReference,
    decimal Amount,
    string Currency,
    string CustomerEmail,
    DateTime PaidAt,
    string RawResponse
);
}
