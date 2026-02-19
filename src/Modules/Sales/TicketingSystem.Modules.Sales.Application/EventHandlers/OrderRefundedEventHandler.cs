using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Application.Commands;
using TicketingSystem.Modules.Sales.Domain.Events;

namespace TicketingSystem.Modules.Sales.Application.EventHandlers
{
    /// <summary>
    /// Handles the actual money movement back to the customer after
    /// Finance has recorded the ledger transaction.
    /// </summary>
    public class OrderRefundedEventHandler : INotificationHandler<OrderRefundedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderRefundedEventHandler> _logger;

        public OrderRefundedEventHandler(
            IMediator mediator,
            ILogger<OrderRefundedEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(
            OrderRefundedEvent notification,
            CancellationToken cancellationToken)
        {
            var command = new ProcessRefundCommand(
                OrderNumber: notification.OrderNumber,
                PaymentReference: notification.PaymentReference,
                Amount: notification.RefundAmount,
                Currency: notification.Currency,
                PaymentGateway: notification.PaymentGateway,
                Reason: notification.RefundReason);

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
            {
               
                _logger.LogError(
                    "Failed to process refund for order {OrderNumber}: {Error}. " +
                    "Ledger transaction is recorded. Manual refund may be required.",
                    notification.OrderNumber,
                    result.Error);
            }
            else
            {
                _logger.LogInformation(
                    "Refund successfully processed for order {OrderNumber}. " +
                    "Refund reference: {RefundReference}",
                    notification.OrderNumber,
                    result.Value);
            }
        }
    }
}
