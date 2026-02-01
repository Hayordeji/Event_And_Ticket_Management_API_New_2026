using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Commands
{
    public record CancelOrderCommand(
    string OrderNumber,
    string CancellationReason
) : IRequest<Result>;
}
