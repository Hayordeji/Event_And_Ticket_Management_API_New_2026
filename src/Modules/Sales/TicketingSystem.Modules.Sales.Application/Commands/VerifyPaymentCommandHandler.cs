using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
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

        public VerifyPaymentCommandHandler(
            SalesDbContext context,
            IEnumerable<IPaymentGatewayService> gatewayServices)
        {
            _context = context;
            _gatewayServices = gatewayServices;
        }

        public async Task<Result> Handle(
            VerifyPaymentCommand request,
            CancellationToken cancellationToken)
        {
            // IDEMPOTENCY: Check if payment already verified
            var existingPayment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.PaymentReference == request.PaymentReference, cancellationToken);

            if (existingPayment == null)
                return Result.Failure("Payment not found.");

            // IDEMPOTENCY: If already completed, return success
            if (existingPayment.Status == PaymentStatus.Successful)
                return Result.Success(); // Already processed (idempotent)

            // Get gateway service
            var gatewayService = _gatewayServices
                .FirstOrDefault(g => g.GatewayName.Equals(request.Gateway, StringComparison.OrdinalIgnoreCase));

            if (gatewayService == null)
                return Result.Failure($"Payment gateway '{request.Gateway}' is not supported.");

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
                    return Result.Failure("Payment not found.");

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


            //try
            //{
            //    await strategy.ExecuteAsync(async () =>
            //    {
            //        var order = existingPayment.Order;

            //        // IDEMPOTENCY: Check order status before updating
            //        if (order.Status == OrderStatus.Paid)
            //        {
            //            await transaction.CommitAsync(cancellationToken);
            //            return Result.Success(); // Already paid (idempotent)
            //        }

            //        if (verification.IsSuccessful)
            //        {
            //            // Mark payment as completed
            //            existingPayment.MarkAsCompleted(
            //                request.PaymentReference,
            //                verification.RawResponse
            //            );

            //            // Mark order as paid (raises OrderPaidEvent)
            //            order.MarkAsPaid(
            //                request.PaymentReference
            //            //verification.RawResponse
            //            );
            //        }
            //        else
            //        {
            //            // Mark payment as failed
            //            existingPayment.MarkAsFailed(
            //                "Payment verification failed",
            //                verification.RawResponse
            //            );
            //        }

            //        await _context.SaveChangesAsync(cancellationToken);
            //        await transaction.CommitAsync(cancellationToken);

                    

            //    });
            //    return verification.IsSuccessful
            //            ? Result.Success()
            //            : Result.Failure("Payment verification failed.");
            //}
            //catch (Exception ex)
            //{
            //    await transaction.RollbackAsync(cancellationToken);
            //    throw;
            //}
        }
    }
}
