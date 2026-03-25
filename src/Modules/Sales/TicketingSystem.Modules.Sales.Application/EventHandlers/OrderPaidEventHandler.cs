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
    /// Handles OrderPaidEvent by sending order confirmation email to customer
    /// </summary>
    public class OrderPaidEventHandler : INotificationHandler<OrderPaidEvent>
    {
        private readonly SalesDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderPaidEventHandler> _logger;

        public OrderPaidEventHandler(
            SalesDbContext context,
            IEmailService emailService,
            ILogger<OrderPaidEventHandler> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(
            OrderPaidEvent notification,
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
                        "Order not found for OrderPaidEvent. OrderId={OrderId}, OrderNumber={OrderNumber}",
                        notification.OrderId,
                        notification.OrderNumber);
                    return;
                }

                // Verify order is in expected state
                if (order.Status != Domain.Enums.OrderStatus.Paid)
                {
                    _logger.LogWarning(
                        "Order status is not Paid. OrderId={OrderId}, Status={Status}",
                        notification.OrderId,
                        order.Status);
                    return;
                }

                _logger.LogInformation(
                    "Sending order confirmation email for order {OrderNumber} to {Email}",
                    notification.OrderNumber,
                    order.CustomerEmail);

                // Send confirmation email
                var emailResult = await _emailService.SendOrderConfirmationEmailAsync(
                    recipientEmail: order.CustomerEmail,
                    recipientName: order.CustomerName,
                    orderNumber: notification.OrderNumber,
                    eventName: order.EventName,
                    eventDate: order.EventStartDate,
                    venueName: order.VenueName,
                    totalAmount: notification.TotalAmount,
                    ticketCount: order.GetTotalTicketCount(),
                    ct: cancellationToken);

                if (!emailResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "Failed to send order confirmation email for order {OrderNumber} to {Email}. Response: {Response}",
                        notification.OrderNumber,
                        order.CustomerEmail,
                        emailResult.Response);
                }
                else
                {
                    _logger.LogInformation(
                        "Order confirmation email sent successfully for order {OrderNumber}. MessageId={MessageId}",
                        notification.OrderNumber,
                        emailResult.MessageId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing OrderPaidEvent for order {OrderNumber}",
                    notification.OrderNumber);
                // Don't throw — let Outbox handle the retry
            }
        }
    }
}