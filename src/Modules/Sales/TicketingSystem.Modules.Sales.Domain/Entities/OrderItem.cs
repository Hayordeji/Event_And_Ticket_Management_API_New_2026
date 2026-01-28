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
        public Guid EventId { get; private set; }
        public OrderNumber OrderNumber { get; set; }
        public Guid TicketTypeId { get; private set; }
        public string EventName { get; private set; } = string.Empty;
        public string TicketTypeName { get; private set; } = string.Empty;
        public Money UnitPrice { get; private set; } = null!;
        public int Quantity { get; private set; }
        public Money Subtotal { get; private set; } = null!;

        // Snapshot of event/ticket details at purchase time (immutability)
        public DateTime EventStartDate { get; private set; }
        public string VenueName { get; private set; } = string.Empty;
        public string VenueCity { get; private set; } = string.Empty;

        // Private constructor for EF Core
        private OrderItem() { }

        private OrderItem(
            Guid eventId,
            Guid ticketTypeId,
            string eventName,
            string ticketTypeName,
            Money unitPrice,
            int quantity,
            DateTime eventStartDate,
            string venueName,
            string venueCity)
        {
            EventId = eventId;
            TicketTypeId = ticketTypeId;
            EventName = eventName;
            TicketTypeName = ticketTypeName;
            UnitPrice = unitPrice;
            Quantity = quantity;
            Subtotal = Money.Create(unitPrice.Amount * quantity, unitPrice.Currency).Value;
            EventStartDate = eventStartDate;
            VenueName = venueName;
            VenueCity = venueCity;
        }

        /// <summary>
        /// Create order item with validation
        /// </summary>
        public static Result<OrderItem> Create(
            Guid eventId,
            Guid ticketTypeId,
            string eventName,
            string ticketTypeName,
            Money unitPrice,
            int quantity,
            DateTime eventStartDate,
            string venueName,
            string venueCity)
        {
            // Validation
            if (eventId == Guid.Empty)
                return Result.Failure<OrderItem>("Event ID is required");

            if (ticketTypeId == Guid.Empty)
                return Result.Failure<OrderItem>("Ticket type ID is required");

            if (string.IsNullOrWhiteSpace(eventName))
                return Result.Failure<OrderItem>("Event name is required");

            if (string.IsNullOrWhiteSpace(ticketTypeName))
                return Result.Failure<OrderItem>("Ticket type name is required");

            if (unitPrice.Amount <= 0)
                return Result.Failure<OrderItem>("Unit price must be greater than zero");

            if (quantity <= 0)
                return Result.Failure<OrderItem>("Quantity must be greater than zero");

            if (quantity > 100)
                return Result.Failure<OrderItem>("Cannot purchase more than 100 tickets in a single order item");

            if (eventStartDate < DateTime.UtcNow)
                return Result.Failure<OrderItem>("Cannot purchase tickets for past events");

            if (string.IsNullOrWhiteSpace(venueName))
                return Result.Failure<OrderItem>("Venue name is required");

            if (string.IsNullOrWhiteSpace(venueCity))
                return Result.Failure<OrderItem>("Venue city is required");

            var orderItem = new OrderItem(
                eventId,
                ticketTypeId,
                eventName.Trim(),
                ticketTypeName.Trim(),
                unitPrice,
                quantity,
                eventStartDate,
                venueName.Trim(),
                venueCity.Trim());

            return Result.Success(orderItem);
        }

        /// <summary>
        /// Calculate line total (for display purposes - same as Subtotal)
        /// </summary>
        public Money CalculateTotal() => Subtotal;
    }
}
