using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.Modules.Sales.Application.Services;
using TicketingSystem.Modules.Sales.Domain.Entities;
using TicketingSystem.Modules.Sales.Domain.Enums;
using TicketingSystem.Modules.Sales.Domain.Repositories;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Sales.Application.Commands
{
    public class InitializePaymentCommandHandler : IRequestHandler<InitializePaymentCommand, Result<PaymentInitializationResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly SalesDbContext _context;
        private readonly IEnumerable<IPaymentGatewayService> _gatewayServices;
        private readonly ILogger<InitializePaymentCommandHandler> _logger;

        public InitializePaymentCommandHandler(
            IOrderRepository orderRepository,
            SalesDbContext context,
            IEnumerable<IPaymentGatewayService> gatewayServices,
            ILogger<InitializePaymentCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _context = context;
            _gatewayServices = gatewayServices;
            _logger = logger;
        }

        public async Task<Result<PaymentInitializationResponse>> Handle(
            InitializePaymentCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing payment for order {OrderNumber}",request.OrderNumber);

            // Get order with items and payments
            var order = await _orderRepository.GetByOrderNumberAsync(
                request.OrderNumber,
                cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderNumber} not found", request.OrderNumber);
                throw new NotFoundException(nameof(Order), request.OrderNumber);
            }

            // Validate order can accept payment
            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogWarning("Cannot initialize payment for order {OrderNumber}. Order status is {Status}",order.OrderNumber, order.Status);
                return Result.Failure<PaymentInitializationResponse>(
                    $"Cannot initialize payment for order with status: {order.Status}");
            }

            // Check expiration
            if ( order.ExpiresAt < DateTime.UtcNow)
            {
                order.MarkAsExpired();
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Failure<PaymentInitializationResponse>("Order has expired.");
            }

            // IDEMPOTENCY CHECK: Return existing pending payment session if exists
            var existingPendingPayment = order.Payments
                .Where(p => p.Status == PaymentStatus.Pending)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault(p => p.CreatedAt > DateTime.UtcNow.AddMinutes(-15));

            if (existingPendingPayment != null && !string.IsNullOrEmpty(existingPendingPayment.PaymentReference))
            {
                var gatewayResponseObject = JsonSerializer.Deserialize<PaymentInitializationResponse>(existingPendingPayment.GatewayResponse);
                // Return existing payment session (idempotent)

                _logger.LogInformation(
                  "Returning existing payment session for order {OrderNumber}. PaymentReference={PaymentReference}",
                  order.OrderNumber, existingPendingPayment.PaymentReference);

                return Result.Success(new PaymentInitializationResponse(
                    PaymentReference: existingPendingPayment.PaymentReference,
                    AuthorizationUrl: gatewayResponseObject.AuthorizationUrl,
                    AccessCode: gatewayResponseObject.AccessCode
                ));
            }

            // Get gateway service
            var gatewayService = _gatewayServices
                .FirstOrDefault(g => g.GatewayName.Equals(request.Gateway, StringComparison.OrdinalIgnoreCase));

            if (gatewayService == null)
                return Result.Failure<PaymentInitializationResponse>(
                    $"Payment gateway '{request.Gateway}' is not supported.");

            // Initialize payment with gateway
            var initializationResult = await gatewayService.InitializePaymentAsync(
                orderNumber: order.OrderNumber,
                amount: order.GrandTotal.Amount,
                currency: order.GrandTotal.Currency,
                customerEmail: request.CustomerEmail,
                callbackUrl: request.CallbackUrl,
                cancellationToken: cancellationToken
            );

            if (!initializationResult.IsSuccess)
                return Result.Failure<PaymentInitializationResponse>(initializationResult.Error);

            _logger.LogInformation(
               "Payment gateway initialization successful. PaymentReference={PaymentReference}",
               initializationResult.Value.PaymentReference);


            string gatewayResponse = JsonSerializer.Serialize(initializationResult.Value);
            // Create pending payment record
            var payment = Payment.Create(
                orderId: order.Id,
                amount: order.GrandTotal.Amount,
                currency: order.GrandTotal.Currency,
                method: PaymentMethod.Card, // Default to Card, will be updated on verification
                paymentReference: initializationResult.Value.PaymentReference,
                gatewayResponse
            );

            // Use reflection to set PaymentReference (since it's private set)
            var paymentReferenceProperty = typeof(Payment).GetProperty(nameof(Payment.PaymentReference));
            var addResult = order.AddPayment(payment);
            _logger.LogInformation(
                "Payment record created for order {OrderNumber}. PaymentId={PaymentId}, PaymentReference={PaymentReference}",
                order.OrderNumber, payment.Id, payment.PaymentReference);

            if (!addResult.IsSuccess)
                return Result.Failure<PaymentInitializationResponse>(addResult.Error);

            //_context.Attach(payment);
            await _context.SaveChangesAsync(cancellationToken);


            return Result.Success(initializationResult.Value);
        }
    }
}
