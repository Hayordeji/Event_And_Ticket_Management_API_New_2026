using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Commands
{
    public record ProcessRefundCommand(
    string OrderNumber,
    string PaymentReference,
    string PaymentGateway, // "Paystack" or "Flutterwave"
    decimal Amount,
    string Currency,
    string Reason
) : IRequest<Result<string>>;
}
