using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Application.Commands;
using TicketingSystem.Modules.Finance.Application.DTOs;
using TicketingSystem.Modules.Finance.Application.Services;
using TicketingSystem.Modules.Finance.Domain.Entities;
using TicketingSystem.Modules.Finance.Domain.Enums;
using TicketingSystem.Modules.Finance.Infrastructure.Persistence;
using TicketingSystem.Modules.Sales.Domain.Events;
using TicketingSystem.SharedKernel.Finance;

namespace TicketingSystem.Modules.Finance.Infrastructure.EventHandlers
{
    public class OrderCancelledEventHandler : INotificationHandler<OrderCancelledEvent>
    {
        private readonly IMediator _mediator;
        private readonly IEventHostService _eventHostService;
        private readonly FinanceDbContext _context;

        public OrderCancelledEventHandler(
            IMediator mediator,
            IEventHostService eventHostService,
            FinanceDbContext context)
        {
            _mediator = mediator;
            _eventHostService = eventHostService;
            _context = context;
        }

        public async Task Handle(
            OrderCancelledEvent notification,
            CancellationToken cancellationToken)
        {
            // Idempotency — check by ReferenceType + ReferenceId
            var alreadyReversed = await _context.LedgerTransactions
                .AnyAsync(
                    t => t.ReferenceType == LedgerReferenceTypes.OrderCancellation
                      && t.ReferenceId == notification.OrderId,
                    cancellationToken);

            if (alreadyReversed)
                return;

            // Resolve HostId from EventId
            var hostIdResult = await _eventHostService
                .GetHostIdAsync(notification.HostEventId, cancellationToken);

            if (hostIdResult.IsFailure)
                throw new InvalidOperationException(
                    $"Cannot process cancellation reversal: {hostIdResult.Error}");

            var hostAccountCode =
                $"LIA-HOST-{hostIdResult.Value.ToString()[..8].ToUpper()}";

            var platformCommission = notification.ServiceFee;
            var hostEarnings = notification.GrandTotal - platformCommission;

            var command = new RecordTransactionCommand(
                ReferenceType: LedgerReferenceTypes.OrderCancellation,
                ReferenceId: notification.OrderId,
                Description: $"Cancellation reversal for order {notification.OrderNumber}",
                OccurredAt: notification.CancelledAt,
                Entries:
                [
                    new TransactionEntryRequest(
                    AccountCode: hostAccountCode,
                    Amount: hostEarnings,
                    Currency: notification.Currency,
                    EntryType: EntryType.Debit,
                    Description: LedgerDescriptions.HostEarningsReversal(notification.OrderNumber)),

                new TransactionEntryRequest(
                    AccountCode: "REV-PLATFORM",
                    Amount: platformCommission,
                    Currency: notification.Currency,
                    EntryType: EntryType.Debit,
                    Description: LedgerDescriptions.PlatformCommissionReversal(notification.OrderNumber)),

                new TransactionEntryRequest(
                    AccountCode: "AST-GATEWAY",
                    Amount: notification.GrandTotal,
                    Currency: notification.Currency,
                    EntryType: EntryType.Credit,
                    Description: LedgerDescriptions.GatewayCreditForCancellation(notification.OrderNumber))
                ]);

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                throw new InvalidOperationException(
                    $"Failed to record cancellation reversal: {result.Error}");
        }
    }
}
