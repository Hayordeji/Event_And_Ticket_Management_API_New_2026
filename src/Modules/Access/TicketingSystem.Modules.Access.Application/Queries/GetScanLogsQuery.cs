using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Access.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Access.Application.Queries
{
    public sealed record GetScanLogsQuery(Guid EventId) : IRequest<Result<List<ScanLogResponse>>>;

}
