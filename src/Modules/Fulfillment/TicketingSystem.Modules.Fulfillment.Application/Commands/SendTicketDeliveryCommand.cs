using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Fulfillment.Application.Commands
{
    public record SendTicketDeliveryCommand(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string RecipientEmail,
    string RecipientName,
    List<Guid> TicketIds) : IRequest<Result<Guid>>;
}
    