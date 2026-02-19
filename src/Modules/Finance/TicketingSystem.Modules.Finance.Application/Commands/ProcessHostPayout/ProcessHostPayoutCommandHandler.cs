using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Application.DTOs;
using TicketingSystem.Modules.Finance.Application.Services;
using TicketingSystem.Modules.Finance.Domain.Enums;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Finance;

namespace TicketingSystem.Modules.Finance.Application.Commands.ProcessHostPayout
{
    public class ProcessHostPayoutCommandHandler : IRequestHandler<ProcessHostPayoutCommand, Result<string>>
    {
        private readonly IMediator _mediator;
        private readonly IHostBalanceService _balanceService;
        private readonly IPayoutService _payoutService;
        private readonly ILogger<ProcessHostPayoutCommandHandler> _logger;

        public ProcessHostPayoutCommandHandler(
            IMediator mediator,
            IHostBalanceService balanceService,
            IPayoutService payoutService,
            ILogger<ProcessHostPayoutCommandHandler> logger)
        {
            _mediator = mediator;
            _balanceService = balanceService;
            _payoutService = payoutService;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(
            ProcessHostPayoutCommand request,
            CancellationToken cancellationToken)
        {
            // Step 1: Get host's current balance
            var balanceResult = await _balanceService.GetHostBalanceAsync(
                request.HostId,
                cancellationToken);

            if (balanceResult.IsFailure)
                return Result.Failure<string>(balanceResult.Error);

            var balance = balanceResult.Value;

            if (balance.Balance <= 0)
                return Result.Failure<string>(
                    $"Host balance is {balance.Balance} {balance.Currency}. Nothing to pay out.");

            // Step 2: Initiate payout via payment gateway
            _logger.LogInformation(
                "Initiating payout for host {HostId}, amount {Amount} {Currency}",
                request.HostId,
                balance.Balance,
                balance.Currency);

            var payoutResult = await _payoutService.InitiatePayoutAsync(
                request.HostId,
                balance.Balance,
                balance.Currency,
                request.BankAccountNumber,
                request.BankCode,
                request.Reason,
                cancellationToken);

            if (payoutResult.IsFailure)
            {
                _logger.LogError(
                    "Payout failed for host {HostId}: {Error}",
                    request.HostId,
                    payoutResult.Error);
                return Result.Failure<string>(payoutResult.Error);
            }

            // Step 3: Record ledger transaction
            var transactionCommand = new RecordTransactionCommand(
                ReferenceType: LedgerReferenceTypes.HostPayout,
                ReferenceId: request.HostId,
                Description: $"Payout to host {request.HostId}",
                OccurredAt: DateTime.UtcNow,
                Entries:
                [
                    new TransactionEntryRequest(
                    AccountCode: balance.AccountCode,
                    Amount: balance.Balance,
                    Currency: balance.Currency,
                    EntryType: EntryType.Debit,
                    Description: LedgerDescriptions.HostPayoutProcessed(
                        request.HostId.ToString(),
                        balance.Balance,
                        balance.Currency)),

                new TransactionEntryRequest(
                    AccountCode: "AST-BANK",
                    Amount: balance.Balance,
                    Currency: balance.Currency,
                    EntryType: EntryType.Credit,
                    Description: LedgerDescriptions.HostPayoutCompleted(
                        request.HostId.ToString()))
                ]);

            var transactionResult = await _mediator.Send(transactionCommand, cancellationToken);

            if (transactionResult.IsFailure)
            {
                _logger.LogCritical(
                    "Payout succeeded with gateway but ledger transaction failed for host {HostId}. " +
                    "Payout reference: {PayoutReference}. Manual reconciliation required.",
                    request.HostId,
                    payoutResult.Value.PayoutReference);

                return Result.Failure<string>(
                    "Payout processed but ledger update failed. Contact support.");
            }

            _logger.LogInformation(
                "Payout completed for host {HostId}. Payout reference: {PayoutReference}",
                request.HostId,
                payoutResult.Value.PayoutReference);

            return Result.Success(payoutResult.Value.PayoutReference);
        }
    }
}
