using MediatR;
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

        public InitializePaymentCommandHandler(
            IOrderRepository orderRepository,
            SalesDbContext context,
            IEnumerable<IPaymentGatewayService> gatewayServices)
        {
            _orderRepository = orderRepository;
            _context = context;
            _gatewayServices = gatewayServices;
        }

        public async Task<Result<PaymentInitializationResponse>> Handle(
            InitializePaymentCommand request,
            CancellationToken cancellationToken)
        {
            // Get order with items and payments
            var order = await _orderRepository.GetByOrderNumberAsync(
                request.OrderNumber,
                cancellationToken);

            if (order == null)
                throw new NotFoundException(nameof(Order), request.OrderNumber);

            // Validate order can accept payment
            if (order.Status != OrderStatus.Pending)
                return Result.Failure<PaymentInitializationResponse>(
                    $"Cannot initialize payment for order with status: {order.Status}");

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


            if (!addResult.IsSuccess)
                return Result.Failure<PaymentInitializationResponse>(addResult.Error);

            // Update the last payment's reference
            var lastPayment = order.Payments.OrderByDescending(p => p.CreatedAt).First();

            var paymentEntry = _context.Payments.Entry(payment);
            Console.WriteLine($"Payment state: {paymentEntry.State}");

            //_context.Attach(payment);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(initializationResult.Value);
        }
    }
}
