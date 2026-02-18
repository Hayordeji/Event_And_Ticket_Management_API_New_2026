using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Fulfillment.Application.Queries
{
    public sealed record GetTicketByIdQuery(Guid TicketId, Guid requesterId) : IRequest<Result<TicketResponse>>;

}
