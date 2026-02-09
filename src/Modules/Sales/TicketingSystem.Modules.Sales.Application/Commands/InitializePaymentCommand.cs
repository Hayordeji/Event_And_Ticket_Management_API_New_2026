using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Commands
{
    public record InitializePaymentCommand(
    string OrderNumber,
    string Gateway,  // "Paystack" or "Flutterwave"
    string CustomerEmail,
    string CallbackUrl
) : IRequest<Result<PaymentInitializationResponse>>;
}
