using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Application.DTOs;
using TicketingSystem.Modules.Fulfillment.Application.Queries;
using TicketingSystem.Modules.Fulfillment.Application.Services;
using TicketingSystem.Modules.Fulfillment.Domain.Repositories;
using TicketingSystem.SharedKernel.ApiResponses;

namespace TicketingSystem.Modules.Fulfillment.Api.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ITicketRepository _ticketRepository;
        private readonly IQrCodeGenerator _qrCodeGenerator;
        private readonly IPdfTicketGenerator _pdfGenerator;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(
            IMediator mediator,
            ITicketRepository ticketRepository,
            IQrCodeGenerator qrCodeGenerator,
            IPdfTicketGenerator pdfGenerator,
            ILogger<TicketsController> logger)
        {
            _mediator = mediator;
            _ticketRepository = ticketRepository;
            _qrCodeGenerator = qrCodeGenerator;
            _pdfGenerator = pdfGenerator;
            _logger = logger;
        }

        /// <summary>
        /// Get all tickets for a specific order
        /// </summary>
        [HttpGet("order/{orderNumber}")]
        [ProducesResponseType(typeof(ApiResponse<List<TicketResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTicketsByOrder(
            string orderNumber,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("API request: Get tickets for order {OrderNumber}", orderNumber);

            var query = new GetTicketsByOrderQuery(orderNumber);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(ApiResponse.ErrorResponse(result.Error));

            return Ok(ApiResponse.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Get a single ticket by ID
        /// </summary>
        [HttpGet("{ticketId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TicketResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTicketById(
            Guid ticketId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("API request: Get ticket {TicketId}", ticketId);

            var query = new GetTicketByIdQuery(ticketId);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(ApiResponse.ErrorResponse(result.Error));

            return Ok(ApiResponse.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Get all tickets for the authenticated customer
        /// </summary>
        [HttpGet("my-tickets")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<TicketResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyTickets(CancellationToken cancellationToken)
        {
            // TODO: Get customer ID from JWT claims
            // For now, using a placeholder
            if (!Guid.TryParse(User.FindFirst("userId")?.Value, out var customerId))
            {
                return Unauthorized();
            }

            _logger.LogInformation("API request: Get tickets for customer {CustomerId}", customerId);

            var query = new GetCustomerTicketsQuery(customerId);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(ApiResponse.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Download ticket as PDF
        /// </summary>
        [HttpGet("{ticketId:guid}/download")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadTicket(
            Guid ticketId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("API request: Download ticket {TicketId}", ticketId);

            var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);

            if (ticket == null)
            {
                _logger.LogWarning("Ticket {TicketId} not found for download", ticketId);
                return NotFound(ApiResponse.ErrorResponse("Ticket not found"));
            }

            var pdfBytes = _pdfGenerator.GenerateTicketPdf(ticket);

            _logger.LogInformation(
                "Ticket {TicketNumber} downloaded successfully. Size={Size} bytes",
                ticket.TicketNumber, pdfBytes.Length);

            return File(pdfBytes, "application/pdf", $"Ticket-{ticket.TicketNumber}.pdf");
        }

        /// <summary>
        /// Get QR code image for a ticket
        /// </summary>
        [HttpGet("{ticketId:guid}/qr")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTicketQrCode(
            Guid ticketId,
            [FromQuery] int size = 300,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("API request: Get QR code for ticket {TicketId}", ticketId);

            var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);

            if (ticket == null)
            {
                _logger.LogWarning("Ticket {TicketId} not found for QR code generation", ticketId);
                return NotFound(ApiResponse.ErrorResponse("Ticket not found"));
            }

            var pixelsPerModule = size / 20; // Calculate appropriate pixel density
            var qrCodeBytes = _qrCodeGenerator.GenerateQrCodeImage(ticket.QrCodeData, pixelsPerModule);

            _logger.LogInformation(
                "QR code generated for ticket {TicketNumber}. Size={Size} bytes",
                ticket.TicketNumber, qrCodeBytes.Length);

            return File(qrCodeBytes, "image/png", $"QR-{ticket.TicketNumber}.png");
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            return Ok(ApiResponse.SuccessResponse("Fulfillment API is healthy"));
        }
    }
}
