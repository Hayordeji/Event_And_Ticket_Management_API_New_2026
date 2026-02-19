using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Application.Services;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Commands
{
    public class ProcessRefundCommandHandler : IRequestHandler<ProcessRefundCommand, Result<string>>
    {
        private readonly IEnumerable<IPaymentGatewayRefundService> _refundServices;
        private readonly ILogger<ProcessRefundCommandHandler> _logger;

        public ProcessRefundCommandHandler(
            IEnumerable<IPaymentGatewayRefundService> refundServices,
            ILogger<ProcessRefundCommandHandler> logger)
        {
            _refundServices = refundServices;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(
            ProcessRefundCommand request,
            CancellationToken cancellationToken)
        {
            // Select the appropriate gateway service
            var refundService = _refundServices
                .FirstOrDefault(s => s.GatewayName.Equals(
                    request.PaymentGateway,
                    StringComparison.OrdinalIgnoreCase));

            if (refundService == null)
            {
                _logger.LogError(
                    "No refund service found for gateway {Gateway}",
                    request.PaymentGateway);
                return Result.Failure<string>(
                    $"Payment gateway '{request.PaymentGateway}' not supported for refunds.");
            }

            _logger.LogInformation(
                "Initiating refund via {Gateway} for order {OrderNumber}, amount {Amount} {Currency}",
                request.PaymentGateway,
                request.OrderNumber,
                request.Amount,
                request.Currency);

            var result = await refundService.InitiateRefundAsync(
                request.PaymentReference,
                request.Amount,
                request.Currency,
                request.Reason,
                cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Refund successful for order {OrderNumber}. Refund reference: {RefundReference}",
                    request.OrderNumber,
                    result.Value.RefundReference);

                return Result.Success(result.Value.RefundReference);
            }

            _logger.LogError(
                "Refund failed for order {OrderNumber}: {Error}",
                request.OrderNumber,
                result.Error);

            return Result.Failure<string>(result.Error);
        }
    }
}
