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
    public class GetScanLogsQueryHandler : IRequestHandler<GetScanLogsQuery, Result<List<ScanLogResponse>>>
    {
        private readonly IScanLogRepository _scanLogRepository;
        private readonly ILogger<GetScanLogsQueryHandler> _logger;

        public GetScanLogsQueryHandler(
            IScanLogRepository scanLogRepository,
            ILogger<GetScanLogsQueryHandler> logger)
        {
            _scanLogRepository = scanLogRepository;
            _logger = logger;
        }

        public async Task<Result<List<ScanLogResponse>>> Handle(
            GetScanLogsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving scan logs for EventId={EventId}", request.EventId);

            var logs = await _scanLogRepository.GetByEventIdAsync(request.EventId, cancellationToken);

            var response = logs.Select(l => new ScanLogResponse(
                ScanLogId: l.Id,
                TicketNumber: l.TicketNumber,
                Result: l.Result.ToString(),
                DenialReason: l.DenialReason?.ToString(),
                GateLocation: l.GateLocation,
                DeviceId: l.DeviceId,
                ScannedAt: l.ScannedAt
            )).ToList();

            return Result.Success(response);
        }
    }
}
