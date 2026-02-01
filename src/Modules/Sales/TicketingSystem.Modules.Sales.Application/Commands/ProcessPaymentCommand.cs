using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Domain.Enums;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Commands
{
    public record ProcessPaymentCommand(
    string OrderNumber,
    PaymentMethod PaymentMethod,
    string PaymentReference,
    string GatewayResponse
) : IRequest<Result>;
}
