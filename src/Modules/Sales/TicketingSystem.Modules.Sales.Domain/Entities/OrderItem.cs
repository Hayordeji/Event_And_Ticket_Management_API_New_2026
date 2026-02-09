using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.ValueObjects;
using TicketingSystem.Modules.Sales.Domain.ValueObjects;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Domain.Entities
{
    public class OrderItem : Entity
    {
        public Guid TicketTypeId { get; private set; }

        // Snapshot of ticket details at purchase time (immutability)
        public string TicketTypeName { get; private set; } = string.Empty;
        public string TicketTypeDescription { get; private set; } = string.Empty;
        public Money UnitPrice { get; private set; } = null!;
        public int Quantity { get; private set; }
        public Money Subtotal { get; private set; } = null!;


        // Private constructor for EF Core
        private OrderItem() { }

        private OrderItem(
            Guid eventId,
            Guid ticketTypeId,
            string ticketTypeName,
            string ticketTypeDescription,
            Money unitPrice,
            int quantity
            )
        {
            TicketTypeId = ticketTypeId;
            TicketTypeName = ticketTypeName;
            TicketTypeDescription = ticketTypeDescription;
            UnitPrice = unitPrice;
            Quantity = quantity;
            Subtotal = Money.Create(unitPrice.Amount * quantity, unitPrice.Currency).Value;
        }

        /// <summary>
        /// Create order item with validation
        /// </summary>
        public static Result<OrderItem> Create(
            Guid eventId,
            Guid ticketTypeId,
            string ticketTypeName,
            string ticketTypeDescription,
            Money unitPrice,
            int quantity)
        {
            // Validation
            if (eventId == Guid.Empty)
                return Result.Failure<OrderItem>("Event ID is required");

            if (ticketTypeId == Guid.Empty)
                return Result.Failure<OrderItem>("Ticket type ID is required");

            if (string.IsNullOrWhiteSpace(ticketTypeDescription))
                return Result.Failure<OrderItem>("Ticket description is required");


            if (string.IsNullOrWhiteSpace(ticketTypeName))
                return Result.Failure<OrderItem>("Ticket type name is required");

            if (unitPrice.Amount <= 0)
                return Result.Failure<OrderItem>("Unit price must be greater than zero");

            if (quantity <= 0)
                return Result.Failure<OrderItem>("Quantity must be greater than zero");

            if (quantity > 100)
                return Result.Failure<OrderItem>("Cannot purchase more than 100 tickets in a single order item");   

            var orderItem = new OrderItem(
                eventId,
                ticketTypeId,
                ticketTypeName.Trim(),
                ticketTypeDescription.Trim(),
                unitPrice,
                quantity
            );

            return Result.Success(orderItem);
        }

        /// <summary>
        /// Calculate line total (for display purposes - same as Subtotal)
        /// </summary>
        public Money CalculateTotal() => Subtotal;
    }
}
