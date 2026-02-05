using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Commands
{
    public record VerifyPaymentCommand(
    string PaymentReference,
    string Gateway  // "Paystack" or "Flutterwave"
) : IRequest<Result>;
}
