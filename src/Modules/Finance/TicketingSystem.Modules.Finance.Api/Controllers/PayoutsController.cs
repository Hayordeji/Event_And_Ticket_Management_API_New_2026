using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Application.Commands.ProcessHostPayout;
using TicketingSystem.Modules.Finance.Application.Services;
using TicketingSystem.SharedKernel.Authorization;

namespace TicketingSystem.Modules.Finance.Api.Controllers
{
    [ApiController]
    [Route("api/finance/payouts")]
    [Authorize(Policy = PolicyNames.RequireAdmin)]
    public class PayoutsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHostBalanceService _balanceService;

        public PayoutsController(
            IMediator mediator,
            IHostBalanceService balanceService)
        {
            _mediator = mediator;
            _balanceService = balanceService;
        }

        [HttpGet("balance/{hostId}")]
        [EnableRateLimiting("fixed_get_endpoints")]
        public async Task<IActionResult> GetHostBalance(
            Guid hostId,
            CancellationToken cancellationToken)
        {
            var result = await _balanceService.GetHostBalanceAsync(hostId, cancellationToken);

            if (result.IsFailure)
                return NotFound(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("{hostId}")]
        [EnableRateLimiting("fixed_create_endpoints")]
        public async Task<IActionResult> ProcessPayout(
            Guid hostId,
            [FromBody] ProcessPayoutRequest request,
            CancellationToken cancellationToken)
        {
            var command = new ProcessHostPayoutCommand(
                HostId: hostId,
                BankAccountNumber: request.BankAccountNumber,
                BankCode: request.BankCode,
                Reason: request.Reason ?? $"Weekly payout for host {hostId}");

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(new { PayoutReference = result.Value });
        }

    }
    public record ProcessPayoutRequest(
    string BankAccountNumber,
    string BankCode,
    string? Reason
    );
}
