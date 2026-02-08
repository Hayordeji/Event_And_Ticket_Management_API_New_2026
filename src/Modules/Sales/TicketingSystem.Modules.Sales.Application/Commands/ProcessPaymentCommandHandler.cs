using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Domain.Entities;
using TicketingSystem.Modules.Sales.Domain.Repositories;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Sales.Application.Commands
{
    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly SalesDbContext _context;

        public ProcessPaymentCommandHandler(
            IOrderRepository orderRepository,
            SalesDbContext context)
        {
            _orderRepository = orderRepository;
            _context = context;
        }

        public async Task<Result> Handle(
            ProcessPaymentCommand request,
            CancellationToken cancellationToken)
        {
            // Get order with items and payments
            var order = await _orderRepository.GetByOrderNumberAsync(
                request.OrderNumber,
                cancellationToken);

            if (order == null)
                throw new NotFoundException(nameof(Order), request.OrderNumber);

            // Validate order status
            if (order.Status != Domain.Enums.OrderStatus.Pending)
                return Result.Failure($"Cannot process payment for order with status: {order.Status}");

            // Check if order has expired
            if( order.ExpiresAt < DateTime.UtcNow)
            {
                order.MarkAsExpired();
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Failure("Order has expired.");
            }

            var payment = Payment.Create(order.Id, order.GrandTotal.Amount, order.GrandTotal.Currency, request.PaymentMethod,request.PaymentReference, request.GatewayResponse);
            // Add payment record
            order.AddPayment(
               payment
            );

            // Mark order as paid
            order.MarkAsPaid(
                payment.PaymentReference
            );

            _context.Attach(payment);
            // Save changes
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
