using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Application.Commands;
using TicketingSystem.Modules.Finance.Application.DTOs;
using TicketingSystem.Modules.Finance.Application.Services;
using TicketingSystem.Modules.Finance.Domain.Enums;
using TicketingSystem.Modules.Sales.Domain.Events;
using TicketingSystem.SharedKernel.Finance;


namespace TicketingSystem.Modules.Finance.Application.EventHandlers
{
    public class OrderPaidEventHandler : INotificationHandler<OrderPaidEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderPaidEventHandler> _logger;
        private const decimal PlatformFeePercentage = 0.05m; // 5% platform commission
        private readonly IHostAccountService _hostAccountService;
        private readonly ICatalogQueryService _catalogQueryService;

        public OrderPaidEventHandler(
            IMediator mediator,
            ILogger<OrderPaidEventHandler> logger,
            IHostAccountService hostAccountService,
            ICatalogQueryService catalogQueryService)
        {
            _mediator = mediator;
            _logger = logger;
            _hostAccountService = hostAccountService;
            _catalogQueryService = catalogQueryService;
        }

        public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
        "Processing OrderPaidEvent for order {OrderNumber}. Creating ledger transaction. Amount={Amount} {Currency}",
        notification.OrderNumber, notification.TotalAmount, notification.Currency);

            try
            {
                // Get event data to find the host
                var eventData = await _catalogQueryService.GetEventAsync(notification.HostEventId, cancellationToken);

                if (eventData == null)
                {
                    _logger.LogError("Event {EventId} not found for order {OrderNumber}",
                        notification.EventId, notification.OrderNumber);
                    return;
                }

                var hostId = eventData.HostId;

                _logger.LogDebug("Event {EventId} is hosted by {HostId}", notification.EventId, hostId);

                // Ensure host account exists (BACKUP defense - should already exist from EventPublishedEvent)
                var hostAccountCode = await _hostAccountService.EnsureHostAccountExistsAsync(
                    hostId,
                    eventData.HostName,
                    cancellationToken);

                // Calculate platform fee and host earnings
                var totalAmount = notification.TotalAmount;
                var platformFee = totalAmount * PlatformFeePercentage;
                var hostEarnings = totalAmount - platformFee;

                _logger.LogDebug(
                    "Financial breakdown: Total={Total}, PlatformFee={Fee} (5%), HostEarnings={Earnings}, HostAccount={HostAccount}",
                    totalAmount, platformFee, hostEarnings, hostAccountCode);

                // Create double-entry ledger transaction with HOST-SPECIFIC account
                var command = new RecordTransactionCommand(
                    ReferenceType: "ORDER_PAYMENT",
                    ReferenceId: notification.OrderId,
                    Description: $"Ticket sale for Order {notification.OrderNumber}",
                    OccurredAt: DateTime.UtcNow,
                    Entries: new List<TransactionEntryRequest>
                    {
                // DEBIT: Money received from payment gateway
                new TransactionEntryRequest(
                    AccountCode: "AST-GATEWAY",
                    Amount: totalAmount,
                    Currency: notification.Currency,
                    EntryType: EntryType.Debit,
                    Description: LedgerDescriptions.PaymentReceived(notification.OrderNumber)
                ),
                
                // CREDIT: Platform commission revenue
                new TransactionEntryRequest(
                    AccountCode: "REV-PLATFORM",
                    Amount: platformFee,
                    Currency: notification.Currency,
                    EntryType: EntryType.Credit,
                    Description: LedgerDescriptions.PlatformCommissionEarned(notification.OrderNumber)
                ),
                
                // CREDIT: Host-specific account (CHANGED FROM LIA-HOST-PENDING)
                new TransactionEntryRequest(
                    AccountCode: hostAccountCode,  // DYNAMIC HOST ACCOUNT
                    Amount: hostEarnings,
                    Currency: notification.Currency,
                    EntryType: EntryType.Credit,
                    Description: LedgerDescriptions.HostEarningsRecorded(notification.OrderNumber)
                )
                    }
                );

                var result = await _mediator.Send(command, cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Ledger transaction created for order {OrderNumber}. " +
                        "TransactionId={TransactionId}, HostAccount={HostAccount}, PlatformFee={PlatformFee} {Currency}, HostEarnings={HostEarnings} {Currency}",
                        notification.OrderNumber, result.Value, hostAccountCode, platformFee, notification.Currency, hostEarnings, notification.Currency);
                }
                else
                {
                    _logger.LogError(
                        "Failed to create ledger transaction for order {OrderNumber}. Error: {Error}",
                        notification.OrderNumber, result.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing OrderPaidEvent in Finance module for order {OrderNumber}",
                    notification.OrderNumber);
            }
        }
    }
}
