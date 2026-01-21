using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Application.Commands;
using TicketingSystem.Modules.Finance.Application.DTOs;
using TicketingSystem.Modules.Finance.Application.Queries;
using TicketingSystem.SharedKernel.ApiResponses;

namespace TicketingSystem.Modules.Finance.Api.Controllers
{
    [ApiController]
    [Route("api/finance/ledger")]
    [Authorize] 
    public class LedgerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LedgerController> _logger;

        public LedgerController(IMediator mediator, ILogger<LedgerController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Create a new ledger account
        /// </summary>
        [HttpPost("accounts")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateLedgerAccountRequest request)
        {
            var command = new CreateLedgerAccountCommand(
                request.AccountName,
                request.AccountCode,
                request.AccountType,
                request.Currency,
                request.Description);

            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(ApiResponse<LedgerAccountResponse>.ErrorResponse(
                    result.Error,
                    traceId: HttpContext.TraceIdentifier));

            _logger.LogInformation("Ledger account created: {AccountCode}", request.AccountCode);

            return CreatedAtAction(
                nameof(GetAccount),
                new { accountCode = request.AccountCode },
                ApiResponse<LedgerAccountResponse>.SuccessResponse(result.Value, HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Get ledger account by code
        /// </summary>
        [HttpGet("accounts/{accountCode}")]
        public async Task<IActionResult> GetAccount(string accountCode)
        {
            var query = new GetLedgerAccountQuery(accountCode);
            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return NotFound(ApiResponse<LedgerAccountResponse>.ErrorResponse(
                    result.Error,
                    traceId: HttpContext.TraceIdentifier));

            return Ok(ApiResponse<LedgerAccountResponse>.SuccessResponse(result.Value, HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Record a financial transaction
        /// </summary>
        [HttpPost("transactions")]
        public async Task<IActionResult> RecordTransaction([FromBody] RecordTransactionRequest request)
        {
            var command = new RecordTransactionCommand(
                request.ReferenceType,
                request.ReferenceId,
                request.Description,
                request.Entries);

            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(ApiResponse<TransactionResponse>.ErrorResponse(
                    result.Error,
                    traceId: HttpContext.TraceIdentifier));

            _logger.LogInformation(
                "Transaction recorded: {ReferenceType} - {ReferenceId}",
                request.ReferenceType,
                request.ReferenceId);

            return Ok(ApiResponse<TransactionResponse>.SuccessResponse(result.Value, HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Get transaction by reference
        /// </summary>
        [HttpGet("transactions/{referenceType}/{referenceId:guid}")]
        public async Task<IActionResult> GetTransaction(string referenceType, Guid referenceId)
        {
            var query = new GetTransactionByReferenceQuery(referenceType, referenceId);
            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return NotFound(ApiResponse<TransactionResponse>.ErrorResponse(
                    result.Error,
                    traceId: HttpContext.TraceIdentifier));

            return Ok(ApiResponse<TransactionResponse>.SuccessResponse(result.Value, HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Health check for finance module
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new { module = "Finance", status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
