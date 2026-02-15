using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Access.Application.DTOs;
using TicketingSystem.Modules.Access.Domain.Repositories;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Access.Application.Queries
{
    public class GetEventScanStatsQueryHandler : IRequestHandler<GetEventScanStatsQuery, Result<EventScanStatsResponse>>
    {
        private readonly IScanLogRepository _scanLogRepository;
        private readonly ILogger<GetEventScanStatsQueryHandler> _logger;

        public GetEventScanStatsQueryHandler(
            IScanLogRepository scanLogRepository,
            ILogger<GetEventScanStatsQueryHandler> logger)
        {
            _scanLogRepository = scanLogRepository;
            _logger = logger;
        }

        public async Task<Result<EventScanStatsResponse>> Handle(
            GetEventScanStatsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving scan stats for EventId={EventId}", request.EventId);

            var allowed = await _scanLogRepository.GetAllowedCountAsync(request.EventId, cancellationToken);
            var denied = await _scanLogRepository.GetDeniedCountAsync(request.EventId, cancellationToken);

            return Result.Success(new EventScanStatsResponse(
                EventId: request.EventId,
                TotalAllowed: allowed,
                TotalDenied: denied,
                TotalScans: allowed + denied));
        }
    }
}
