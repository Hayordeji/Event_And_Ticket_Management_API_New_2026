using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Fulfillment.Application.Queries
{
    public sealed record GetTicketsByOrderQuery(string OrderNumber, Guid userId) : IRequest<Result<List<TicketResponse>>>;

}
