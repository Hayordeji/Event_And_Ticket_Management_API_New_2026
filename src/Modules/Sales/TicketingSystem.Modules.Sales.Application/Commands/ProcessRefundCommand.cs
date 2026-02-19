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
    decimal Amount,
    string Currency,
    string PaymentGateway,
    string Reason
) : IRequest<Result<string>>;
}
