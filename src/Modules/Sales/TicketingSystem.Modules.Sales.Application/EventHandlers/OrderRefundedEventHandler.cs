using Azure.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Application.Commands;
using TicketingSystem.Modules.Sales.Domain.Entities;
using TicketingSystem.Modules.Sales.Domain.Events;
using TicketingSystem.Modules.Sales.Domain.Repositories;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

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
        private readonly IOrderRepository _orderRepository;

        public OrderRefundedEventHandler(
            IMediator mediator,
            ILogger<OrderRefundedEventHandler> logger,
            IOrderRepository orderRepository)
        {
            _mediator = mediator;
            _logger = logger;
            _orderRepository = orderRepository;
        }

        public async Task Handle(
            OrderRefundedEvent notification,
            CancellationToken cancellationToken)
        {
            try
            {
                
                var order = await _orderRepository.GetByOrderNumberAsync(notification.OrderNumber, cancellationToken);

                if (order == null)
                {
                    _logger.LogError(
                        "Order with order number {OrderNumber} not found. " +
                        "Refund processing cannot proceed. Ledger transaction is recorded. Manual refund may be required.",
                        notification.OrderNumber);
                    return;
                }

                //Idempotency to prevent double refund
                var successfullPayment = order?.Payments.FirstOrDefault(p => p.PaymentReference == notification.PaymentReference);
                if (successfullPayment?.Status == Domain.Enums.PaymentStatus.Refunded)
                {
                    _logger.LogWarning(
                        "Refund for order {OrderNumber} with payment reference {PaymentReference} has already been processed. " +
                        "Skipping refund processing to avoid duplicate refunds.",
                        notification.OrderNumber,
                        notification.PaymentReference);
                    return;
                }


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

                    throw new Exception("Refund Processing failed!!");
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
            catch (Exception ex)
            {
                _logger.LogError(
                        "An unexpected error occured while processing refund for order {OrderNumber}: {Error}. " +
                        "Ledger transaction is recorded. Manual refund may be required.",
                        notification.OrderNumber,
                        ex.Message);
            }
            
        }
    }
}
