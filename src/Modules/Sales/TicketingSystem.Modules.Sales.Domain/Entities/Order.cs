using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TicketingSystem.Modules.Finance.Domain.ValueObjects;
using TicketingSystem.Modules.Sales.Domain.Enums;
using TicketingSystem.Modules.Sales.Domain.Events;
using TicketingSystem.Modules.Sales.Domain.ValueObjects;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Domain.Entities
{
    public class Order : AggregateRoot
    {
        private readonly List<OrderItem> _items = new();
        private readonly List<Payment> _payments = new();

        public OrderNumber OrderNumber { get; private set; } = null!;
        public Guid EventId { get; set; }
        public Guid CustomerId { get; private set; }
        public string CustomerEmail { get; private set; } = string.Empty;
        public string CustomerName { get; private set; } = string.Empty;
        public string? CustomerPhone { get; private set; }

        public OrderStatus Status { get; private set; }
        public Money TotalAmount { get; private set; } = null!;
        public Money PlatformFee { get; private set; } = null!;
        public Money GrandTotal { get; private set; } = null!;

        public DateTime? PaidAt { get; private set; }
        public DateTime? FulfilledAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public DateTime? RefundedAt { get; private set; }
        public DateTime ExpiresAt { get; private set; }

        public string? CancellationReason { get; private set; }
        public string? RefundReason { get; private set; }

        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

        // Private constructor for EF Core
        private Order() { }

        private Order(
            Guid customerId,
            Guid eventId,
            string customerEmail,
            string customerName,
            string? customerPhone)
        {
            Id = Guid.NewGuid();
            OrderNumber = OrderNumber.Generate();
            CustomerId = customerId;
            EventId = eventId;
            CustomerEmail = customerEmail;
            CustomerName = customerName;
            CustomerPhone = customerPhone;
            Status = OrderStatus.Pending;
            ExpiresAt = DateTime.UtcNow.AddMinutes(30); // 30 min to complete payment
            CreatedAt = DateTime.UtcNow;
            CreatedBy = customerId;
        }

        /// <summary>
        /// Create new order
        /// </summary>
        public static Result<Order> Create(
            Guid customerId,
            Guid eventId,
            string customerEmail,
            string customerName,
            string? customerPhone = null)
        {
            // Validation
            if (customerId == Guid.Empty)
                return Result.Failure<Order>("Customer ID is required");

            if (eventId == Guid.Empty)
                return Result.Failure<Order>("Event ID is required");

            if (string.IsNullOrWhiteSpace(customerEmail))
                return Result.Failure<Order>("Customer email is required");

            if (!IsValidEmail(customerEmail))
                return Result.Failure<Order>("Invalid email format");
            
           
            if (string.IsNullOrWhiteSpace(customerName))
                return Result.Failure<Order>("Customer name is required");

            var order = new Order(customerId,eventId, customerEmail.Trim(), customerName.Trim(), customerPhone?.Trim());

            order.RaiseDomainEvent(new OrderCreatedEvent(
                order.Id,
                eventId,
                customerId,
                order.OrderNumber.Value,
                //order.TotalAmount.Amount,
                //order.TotalAmount.Currency,
                DateTime.UtcNow));

            return Result.Success(order);
        }

        /// <summary>
        /// Add item to order
        /// </summary>
        public Result AddItem(OrderItem item)
        {
            if (Status != OrderStatus.Pending)
                return Result.Failure("Can only add items to pending orders");

            // Check for duplicate ticket type in order
            if (_items.Any(i => i.TicketTypeId == item.TicketTypeId))
                return Result.Failure($"Ticket type '{item.TicketTypeName}' already in order. Update quantity instead.");

            _items.Add(item);
            RecalculateTotals();

            return Result.Success();
        }

        /// <summary>
        /// Calculate order totals with platform fee
        /// Platform fee: 10% of subtotal
        /// </summary>
        public void RecalculateTotals()
        {
            if (_items.Count == 0)
            {
                TotalAmount = Money.Create(0, "NGN").Value;
                PlatformFee = Money.Create(0, "NGN").Value;
                GrandTotal = Money.Create(0, "NGN").Value;
                return;
            }

            var currency = _items.First().UnitPrice.Currency;
            var subtotal = _items.Sum(i => i.Subtotal.Amount);
            var platformFeeAmount = subtotal * 0.10m; // 10% platform fee
            var grandTotal = subtotal + platformFeeAmount;

            TotalAmount = Money.Create(subtotal, currency).Value;
            PlatformFee = Money.Create(platformFeeAmount, currency).Value;
            GrandTotal = Money.Create(grandTotal, currency).Value;
        }

        /// <summary>
        /// Add payment to order
        /// </summary>
        public Result AddPayment(Payment payment)
        {
            if (Status == OrderStatus.Paid)
                return Result.Failure("Order is already paid");

            if (Status == OrderStatus.Cancelled)
                return Result.Failure("Cannot add payment to cancelled order");

            if (Status == OrderStatus.Expired)
                return Result.Failure("Order has expired");
            _payments.Add(payment);
            return Result.Success();
        }

        /// <summary>
        /// Mark order as paid (called when payment succeeds)
        /// </summary>
        public Result MarkAsPaid(string paymentReference)
        {
            if (Status == OrderStatus.Paid)
                return Result.Failure("Order is already paid");

            if (Status == OrderStatus.Cancelled)
                return Result.Failure("Cannot mark cancelled order as paid");

            if (Status == OrderStatus.Expired)
                return Result.Failure("Cannot mark expired order as paid");

            var payment = _payments.FirstOrDefault(p => p.PaymentReference == paymentReference);
            if (payment == null)
                return Result.Failure("Payment not found in order");

            if (payment.Status != PaymentStatus.Successful)
                return Result.Failure("Payment must be successful to mark order as paid");

            Status = OrderStatus.Paid;
            PaidAt = DateTime.UtcNow;

            RaiseDomainEvent(new OrderPaidEvent(
                Id,
                OrderNumber.Value,
                GrandTotal.Amount,
                GrandTotal.Currency,
                DateTime.UtcNow,
                CustomerId,
                paymentReference
                ));

            return Result.Success();
        }

        /// <summary>
        /// Mark order as fulfilled (called when tickets are generated)
        /// </summary>
        public Result MarkAsFulfilled()
        {
            if (Status != OrderStatus.Paid)
                return Result.Failure("Order must be paid before it can be fulfilled");

            Status = OrderStatus.Fulfilled;
            FulfilledAt = DateTime.UtcNow;

            return Result.Success();
        }

        /// <summary>
        /// Cancel unpaid order
        /// </summary>
        public Result Cancel(string reason)
        {
            if (Status == OrderStatus.Paid)
                return Result.Failure("Cannot cancel paid order. Use Refund instead.");

            if (Status == OrderStatus.Cancelled)
                return Result.Failure("Order is already cancelled");

            if (Status == OrderStatus.Fulfilled)
                return Result.Failure("Cannot cancel fulfilled order");

            Status = OrderStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            CancellationReason = reason?.Trim();

            RaiseDomainEvent(new OrderCancelledEvent(
                Id,
                OrderNumber.Value,
                CustomerId,
                reason,
                DateTime.UtcNow));

            return Result.Success();
        }

        /// <summary>
        /// Refund paid order
        /// </summary>
        public Result Refund(string reason)
        {
            if (Status != OrderStatus.Paid && Status != OrderStatus.Fulfilled)
                return Result.Failure("Only paid or fulfilled orders can be refunded");

            if (Status == OrderStatus.Refunded)
                return Result.Failure("Order is already refunded");

            Status = OrderStatus.Refunded;
            RefundedAt = DateTime.UtcNow;
            RefundReason = reason?.Trim();

            RaiseDomainEvent(new OrderRefundedEvent(
                Id,
                OrderNumber.Value,
                GrandTotal.Amount,
                GrandTotal.Currency,
                CustomerId,
                reason,
                DateTime.UtcNow));

            return Result.Success();
        }

        /// <summary>
        /// Mark order as expired (payment not received in time)
        /// </summary>
        public Result MarkAsExpired()
        {
            if (Status != OrderStatus.Pending)
                return Result.Failure("Only pending orders can be expired");

            if (DateTime.UtcNow < ExpiresAt)
                return Result.Failure("Order has not yet expired");

            Status = OrderStatus.Expired;

            RaiseDomainEvent(new OrderExpiredEvent(
                Id,
                OrderNumber.Value,
                CustomerId,
                DateTime.UtcNow));

            return Result.Success();
        }

        /// <summary>
        /// Check if order has expired
        /// </summary>
        public bool IsExpired() => Status == OrderStatus.Pending && DateTime.UtcNow > ExpiresAt;

        /// <summary>
        /// Get total ticket quantity across all items
        /// </summary>
        public int GetTotalTicketCount() => _items.Sum(i => i.Quantity);

        /// <summary>
        /// Validate email format (basic)
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
