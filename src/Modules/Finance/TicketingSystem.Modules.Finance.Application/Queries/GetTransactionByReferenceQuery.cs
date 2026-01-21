using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Application.Queries
{
    public record GetTransactionByReferenceQuery(
    string ReferenceType,
    Guid ReferenceId) : IRequest<Result<TransactionResponse>>;
}
