using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Sales.Application.DTOs
{
    public record PaymentInitializationResponse(
    string PaymentReference,
    string AuthorizationUrl,
    string AccessCode
);
}
