using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Domain.Events;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;
using TicketingSystem.SharedKernel.Services;

namespace TicketingSystem.Modules.Sales.Application.EventHandlers
{
    /// <summary>
    /// Handles OrderCancelledEvent by sending cancellation confirmation email to customer
    /// </summary>
    public class OrderCancelledEventHandler : INotificationHandler<OrderCancelledEvent>
    {
        private readonly SalesDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderCancelledEventHandler> _logger;

        public OrderCancelledEventHandler(
            SalesDbContext context,
            IEmailService emailService,
            ILogger<OrderCancelledEventHandler> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(
            OrderCancelledEvent notification,
            CancellationToken cancellationToken)
        {
            try
            {
                // Idempotency guard — check if email was already sent
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == notification.OrderId, cancellationToken);

                if (order == null)
                {
                    _logger.LogWarning(
                        "Order not found for OrderCancelledEvent. OrderId={OrderId}, OrderNumber={OrderNumber}",
                        notification.OrderId,
                        notification.OrderNumber);
                    return;
                }

                // Verify order is in expected state
                if (order.Status != Domain.Enums.OrderStatus.Cancelled)
                {
                    _logger.LogWarning(
                        "Order status is not Cancelled. OrderId={OrderId}, Status={Status}",
                        notification.OrderId,
                        order.Status);
                    return;
                }

                _logger.LogInformation(
                    "Sending cancellation email for order {OrderNumber} to {Email}",
                    notification.OrderNumber,
                    order.CustomerEmail);

                // Send cancellation email
                var emailResult = await _emailService.SendOrderCancelledEmailAsync(
                    recipientEmail: order.CustomerEmail,
                    recipientName: order.CustomerName,
                    orderNumber: notification.OrderNumber,
                    eventName: order.EventName,
                    cancellationReason: notification.CancellationReason ?? "No reason provided",
                    ct: cancellationToken);

                if (!emailResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "Failed to send cancellation email for order {OrderNumber} to {Email}. Response: {Response}",
                        notification.OrderNumber,
                        order.CustomerEmail,
                        emailResult.Response);
                }
                else
                {
                    _logger.LogInformation(
                        "Cancellation email sent successfully for order {OrderNumber}. MessageId={MessageId}",
                        notification.OrderNumber,
                        emailResult.MessageId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing OrderCancelledEvent for order {OrderNumber}",
                    notification.OrderNumber);
                // Don't throw — let Outbox handle the retry
            }
        }
    }
}