using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.Modules.Sales.Domain.Entities;
using TicketingSystem.Modules.Sales.Domain.Repositories;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Sales.Application.Queries
{
    public class GetOrderByNumberQueryHandler : IRequestHandler<GetOrderByNumberQuery, Result<OrderResponse>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByNumberQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Result<OrderResponse>> Handle(
            GetOrderByNumberQuery request,
            CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByOrderNumberAsync(
                request.OrderNumber,
                cancellationToken);

            if (order == null)
                throw new NotFoundException(nameof(Order), request.OrderNumber);

            var response = new OrderResponse(
                Id: order.Id,
                OrderNumber: order.OrderNumber,
                CustomerId: order.CustomerId,
                EventId: order.EventId,
                Status: order.Status,
                SubTotal: order.TotalAmount.Amount,
                ServiceFee: order.PlatformFee.Amount,
                GrandTotal: order.GrandTotal.Amount,
                Currency: order.GrandTotal.Currency,
                CreatedAt: order.CreatedAt,
                PaidAt: order.PaidAt,
                ExpiresAt: order.ExpiresAt,
                Items: order.Items.Select(i => new OrderItemResponse(
                    i.Id,
                    i.TicketTypeId,
                    i.Quantity,
                    i.UnitPrice.Amount,
                    i.Subtotal.Amount
                )).ToList(),
                Payments: order.Payments.Select(p => new PaymentResponse(
                    p.Id,
                    p.Amount,
                    p.Currency,
                    p.Method,
                    p.Status,
                    p.PaymentReference,
                    p.PaidAt
                )).ToList()
            );

            return Result.Success(response);
        }
    }
}
