using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Application.Helpers;
using TicketingSystem.Modules.Sales.Application.Services;
using TicketingSystem.Modules.Sales.Domain.Enums;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Commands
{
    public class VerifyPaymentCommandHandler : IRequestHandler<VerifyPaymentCommand, Result>
    {
        private readonly SalesDbContext _context;
        private readonly IEnumerable<IPaymentGatewayService> _gatewayServices;
        private readonly ILogger<VerifyPaymentCommandHandler> _logger;

        public VerifyPaymentCommandHandler(
            SalesDbContext context,
            IEnumerable<IPaymentGatewayService> gatewayServices,
            ILogger<VerifyPaymentCommandHandler> logger)
        {
            _context = context;
            _gatewayServices = gatewayServices;
            _logger = logger;
        }

        public async Task<Result> Handle(
            VerifyPaymentCommand request,
            CancellationToken cancellationToken)
        {
            var maskedRef = Helper.MaskRefNumber(request.PaymentReference);
            _logger.LogInformation("Verifying payment of reference {refNum}", maskedRef);

            // IDEMPOTENCY: Check if payment already verified
            var existingPayment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.PaymentReference == request.PaymentReference, cancellationToken);

            if (existingPayment == null)
            {
                _logger.LogWarning("Payment with reference {refNum} was not found", maskedRef);
                return Result.Failure("Payment not found.");
            }

            // IDEMPOTENCY: If already completed, return success
            if (existingPayment.Status == PaymentStatus.Successful)
                return Result.Success(); // Already processed (idempotent)

            // Get gateway service
            var gatewayService = _gatewayServices
                .FirstOrDefault(g => g.GatewayName.Equals(request.Gateway, StringComparison.OrdinalIgnoreCase));

            if (gatewayService == null)
            {
                _logger.LogWarning("Payment gateway {gateway} is not supported.",request.Gateway);

                return Result.Failure($"Payment gateway '{request.Gateway}' is not supported.");
            }

            // Verify payment with gateway (double-check, don't trust webhook blindly)
            var verificationResult = await gatewayService.VerifyPaymentAsync(
                request.PaymentReference,
                cancellationToken);

            if (!verificationResult.IsSuccess)
                return Result.Failure(verificationResult.Error);

            var verification = verificationResult.Value;

            // Start transaction for consistency
            var strategy =  _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync(cancellationToken);

                var existingPayment = await _context.Payments
                    .Include(p => p.Order)
                    .FirstOrDefaultAsync(
                        p => p.PaymentReference == request.PaymentReference,
                        cancellationToken);

                if (existingPayment == null)
                {
                    _logger.LogWarning("Payment with ref {ref} is not found in the database.", request.PaymentReference);

                    return Result.Failure("Payment not found.");
                }

                if (existingPayment.Status == PaymentStatus.Successful)
                    return Result.Success();

                var order = existingPayment.Order;

                if (order.Status == OrderStatus.Paid)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return Result.Success();
                }

                if (verification.IsSuccessful)
                {
                    existingPayment.MarkAsCompleted(
                        request.PaymentReference,
                        verification.RawResponse);

                    order.MarkAsPaid(request.PaymentReference);
                }
                else
                {
                    existingPayment.MarkAsFailed(
                        "Payment verification failed",
                        verification.RawResponse);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return verification.IsSuccessful
                    ? Result.Success()
                    : Result.Failure("Payment verification failed.");
            });


           
        }
    }
}
