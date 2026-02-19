using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Application.Commands;
using TicketingSystem.Modules.Finance.Application.DTOs;
using TicketingSystem.Modules.Finance.Application.Services;
using TicketingSystem.Modules.Finance.Domain.Enums;
using TicketingSystem.Modules.Finance.Infrastructure.Persistence;
using TicketingSystem.Modules.Sales.Domain.Events;
using TicketingSystem.SharedKernel.Finance;

namespace TicketingSystem.Modules.Finance.Application.EventHandlers
{
    public class OrderRefundedEventHandler : INotificationHandler<OrderRefundedEvent>
    {
        private readonly IMediator _mediator;
        private readonly IOrderEventService _orderEventService;
        private readonly IEventHostService _eventHostService;
        private readonly FinanceDbContext _context;

        public OrderRefundedEventHandler(
            IMediator mediator,
            IOrderEventService orderEventService,
            IEventHostService eventHostService,
            FinanceDbContext context)
        {
            _mediator = mediator;
            _orderEventService = orderEventService;
            _eventHostService = eventHostService;
            _context = context;
        }

        public async Task Handle(
            OrderRefundedEvent notification,
            CancellationToken cancellationToken)
        {
            // Idempotency — check by ReferenceType + ReferenceId
            var alreadyRefunded = await _context.LedgerTransactions
                .AnyAsync(
                    t => t.ReferenceType == LedgerReferenceTypes.OrderRefund
                      && t.ReferenceId == notification.OrderId,
                    cancellationToken);

            if (alreadyRefunded)
                return;

            // Step 1: Resolve EventId from OrderId
            var eventIdResult = await _orderEventService
                .GetEventIdByOrderIdAsync(notification.OrderId, cancellationToken);

            if (eventIdResult.IsFailure)
                throw new InvalidOperationException(
                    $"Cannot process refund: {eventIdResult.Error}");

            // Step 2: Resolve HostId from EventId
            var hostIdResult = await _eventHostService
                .GetHostIdAsync(eventIdResult.Value, cancellationToken);

            if (hostIdResult.IsFailure)
                throw new InvalidOperationException(
                    $"Cannot process refund: {hostIdResult.Error}");

            var hostAccountCode =
                $"LIA-HOST-{hostIdResult.Value.ToString()[..8].ToUpper()}";

            var hostEarnings = notification.RefundAmount - notification.ServiceFee;
            var platformCommission = notification.ServiceFee;

            var command = new RecordTransactionCommand(
                ReferenceType: LedgerReferenceTypes.OrderRefund,
                ReferenceId: notification.OrderId,
                Description: $"Refund for order {notification.OrderNumber}",
                OccurredAt: notification.RefundedAt,
                Entries:
                [
                    new TransactionEntryRequest(
                    AccountCode: hostAccountCode,
                    Amount: hostEarnings,
                    Currency: notification.Currency,
                    EntryType: EntryType.Debit,
                    Description: LedgerDescriptions.HostEarningsRefunded(notification.OrderNumber)),

                new TransactionEntryRequest(
                    AccountCode: "EXP-REFUNDS",
                    Amount: platformCommission,
                    Currency: notification.Currency,
                    EntryType: EntryType.Debit,
                    Description: LedgerDescriptions.PlatformCommissionAbsorbed(notification.OrderNumber)),

                new TransactionEntryRequest(
                    AccountCode: "AST-GATEWAY",
                    Amount: notification.RefundAmount,
                    Currency: notification.Currency,
                    EntryType: EntryType.Credit,
                    Description: LedgerDescriptions.FullRefundToGateway(notification.OrderNumber))
                ]);

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                throw new InvalidOperationException(
                    $"Failed to record refund transaction: {result.Error}");
        }
    }
}
