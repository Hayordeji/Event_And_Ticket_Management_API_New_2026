using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Commands
{
    public record RequestRefundCommand(
    string OrderNumber,
    string Reason
    ) : IRequest<Result<string>>;
}
