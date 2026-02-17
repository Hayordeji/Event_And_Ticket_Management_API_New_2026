using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TicketingSystem.Modules.Access.Application.Commands;
using TicketingSystem.Modules.Access.Application.DTOs;
using TicketingSystem.Modules.Access.Application.Queries;
using TicketingSystem.SharedKernel.ApiResponses;
using TicketingSystem.SharedKernel.Authorization;

namespace TicketingSystem.Modules.Access.Api.Controllers
{
    [Route("api/access")]
    [ApiController]
    [Authorize] 
    public class AccessController : ControllerBase  
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AccessController> _logger;

        public AccessController(
            IMediator mediator,
            ILogger<AccessController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Scan a ticket QR code at venue entrance
        /// </summary>
        [HttpPost("scan")]
        [Authorize(Policy = PolicyNames.RequireScanner)]       
        [ProducesResponseType(typeof(ApiResponse<ScanTicketResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ScanTicket(
            [FromBody] ScanTicketRequest request,
            CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(User.FindFirst("userId")?.Value, out var userId))
            {
                return Unauthorized();
            }

            var scannedBy = userId;

            _logger.LogInformation(
                "Ticket scan request Device={DeviceId}, Gate={Gate}",
                 request.DeviceId, request.GateLocation);

            var command = new ScanTicketCommand(
                QrCodeData: request.QrCodeData,
                ScannedBy: scannedBy,
                DeviceId: request.DeviceId,
                GateLocation: request.GateLocation);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.ErrorResponse(result.Error));

            return Ok(ApiResponse.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Get all scan logs for an event
        /// </summary>
        [HttpGet("logs/{eventId:guid}")]
        [Authorize(Policy = PolicyNames.RequireScanner)]
        [ProducesResponseType(typeof(ApiResponse<List<ScanLogResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetScanLogs(
            Guid eventId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving scan logs for EventId={EventId}", eventId);

            var query = new GetScanLogsQuery(eventId);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(ApiResponse.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Get scan statistics for an event (total entries, denials)
        /// </summary>
        [HttpGet("stats/{eventId:guid}")]
        [Authorize(Policy = PolicyNames.RequireScanner)]
        [ProducesResponseType(typeof(ApiResponse<EventScanStatsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventScanStats(
            Guid eventId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving scan stats for EventId={EventId}", eventId);

            var query = new GetEventScanStatsQuery(eventId);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(ApiResponse.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Health check
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health() => Ok(ApiResponse.SuccessResponse("Access API is healthy"));
    }
}
