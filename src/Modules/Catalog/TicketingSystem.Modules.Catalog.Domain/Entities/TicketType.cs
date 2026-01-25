using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Events;
using TicketingSystem.Modules.Finance.Domain.ValueObjects;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Domain.Entities
{
    ///<summary>
    /// Represents a ticket category for an event (e.g., General Admission, VIP, Early Bird)
    /// </summary>
    public sealed class TicketType : Entity
    {
        public Guid EventId { get; private set; }
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public Money Price { get; private set; }
        public int TotalCapacity { get; private set; }
        public int SoldCount { get; private set; }
        public int ReservedCount { get; private set; }
        public DateTime? SaleStartDate { get; private set; }
        public DateTime? SaleEndDate { get; private set; }
        public int? MinPurchaseQuantity { get; private set; }
        public int? MaxPurchaseQuantity { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core navigation
        public Event Event { get; private set; } = null!;

        // Computed property
        public int AvailableCount => TotalCapacity - SoldCount - ReservedCount;
        public bool IsSoldOut => AvailableCount <= 0;

        // EF Core constructor
        private TicketType() { }

        private TicketType(
            Guid eventId,
            string name,
            string? description,
            Money price,
            int totalCapacity,
            DateTime? saleStartDate,
            DateTime? saleEndDate,
            int? minPurchaseQuantity,
            int? maxPurchaseQuantity)
        {
            EventId = eventId;
            Name = name;
            Description = description;
            Price = price;
            TotalCapacity = totalCapacity;
            SoldCount = 0;
            ReservedCount = 0;
            SaleStartDate = saleStartDate;
            SaleEndDate = saleEndDate;
            MinPurchaseQuantity = minPurchaseQuantity;
            MaxPurchaseQuantity = maxPurchaseQuantity;
            IsActive = true;
        }

        /// <summary>
        /// Factory method to create a new ticket type
        /// </summary>
        public static Result<TicketType> Create(
            Guid eventId,
            string name,
            Money price,
            int totalCapacity,
            string? description = null,
            DateTime? saleStartDate = null,
            DateTime? saleEndDate = null,
            int? minPurchaseQuantity = null,
            int? maxPurchaseQuantity = null)
        {
            // Validation
            if (eventId == Guid.Empty)
                return Result.Failure<TicketType>("Event ID is required");

            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<TicketType>("Ticket type name is required");

            if (name.Length > 100)
                return Result.Failure<TicketType>("Ticket type name cannot exceed 100 characters");

            if (price.Amount < 0)
                return Result.Failure<TicketType>("Price cannot be negative");

            if (totalCapacity <= 0)
                return Result.Failure<TicketType>("Total capacity must be greater than zero");

            if (totalCapacity > 1_000_000)
                return Result.Failure<TicketType>("Total capacity cannot exceed 1,000,000");

            if (saleStartDate.HasValue && saleEndDate.HasValue && saleStartDate >= saleEndDate)
                return Result.Failure<TicketType>("Sale start date must be before sale end date");

            if (minPurchaseQuantity.HasValue && minPurchaseQuantity < 1)
                return Result.Failure<TicketType>("Minimum purchase quantity must be at least 1");

            if (maxPurchaseQuantity.HasValue && maxPurchaseQuantity < 1)
                return Result.Failure<TicketType>("Maximum purchase quantity must be at least 1");

            if (minPurchaseQuantity.HasValue && maxPurchaseQuantity.HasValue
                && minPurchaseQuantity > maxPurchaseQuantity)
                return Result.Failure<TicketType>("Minimum purchase quantity cannot exceed maximum");

            var ticketType = new TicketType(
                eventId,
                name.Trim(),
                description?.Trim(),
                price,
                totalCapacity,
                saleStartDate,
                saleEndDate,
                minPurchaseQuantity,
                maxPurchaseQuantity);

            return Result.Success(ticketType);
        }

        /// <summary>
        /// Check if this ticket type is available for purchase at the given date/time
        /// </summary>
        public bool IsAvailableAt(DateTime checkDate)
        {
            if (!IsActive) return false;
            if (IsSoldOut) return false;

            if (SaleStartDate.HasValue && checkDate < SaleStartDate.Value)
                return false;

            if (SaleEndDate.HasValue && checkDate > SaleEndDate.Value)
                return false;

            return true;
        }

        /// <summary>
        /// Reserve tickets (for pending orders)
        /// </summary>
        public Result ReserveTickets(int quantity)
        {
            if (quantity <= 0)
                return Result.Failure("Quantity must be greater than zero");

            if (quantity > AvailableCount)
                return Result.Failure($"Only {AvailableCount} tickets available");

            if (MaxPurchaseQuantity.HasValue && quantity > MaxPurchaseQuantity.Value)
                return Result.Failure($"Maximum purchase quantity is {MaxPurchaseQuantity.Value}");

            if (MinPurchaseQuantity.HasValue && quantity < MinPurchaseQuantity.Value)
                return Result.Failure($"Minimum purchase quantity is {MinPurchaseQuantity.Value}");

            ReservedCount += quantity;
            return Result.Success();
        }

        /// <summary>
        /// Release reserved tickets (when order cancelled/expired)
        /// </summary>
        public Result ReleaseReservation(int quantity)
        {
            if (quantity <= 0)
                return Result.Failure("Quantity must be greater than zero");

            if (quantity > ReservedCount)
                return Result.Failure("Cannot release more tickets than reserved");

            ReservedCount -= quantity;
            return Result.Success();
        }

        /// <summary>
        /// Mark tickets as sold (when order confirmed)
        /// </summary>
        public Result MarkAsSold(int quantity)
        {
            if (quantity <= 0)
                return Result.Failure("Quantity must be greater than zero");

            if (quantity > ReservedCount)
                return Result.Failure("Cannot sell more tickets than reserved");

            ReservedCount -= quantity;
            SoldCount += quantity;
            return Result.Success();
        }

        /// <summary>
        /// Update ticket type details
        /// </summary>
        public Result Update(
            string? name = null,
            string? description = null,
            Money? price = null,
            DateTime? saleStartDate = null,
            DateTime? saleEndDate = null,
            bool? isActive = null)
        {
            if (name != null)
            {
                if (string.IsNullOrWhiteSpace(name))
                    return Result.Failure("Ticket type name cannot be empty");

                if (name.Length > 100)
                    return Result.Failure("Ticket type name cannot exceed 100 characters");

                Name = name.Trim();
            }

            if (description != null)
                Description = description.Trim();

            if (price != null)
            {
                if (price.Amount < 0)
                    return Result.Failure("Price cannot be negative");

                Price = price;
            }

            if (saleStartDate.HasValue)
                SaleStartDate = saleStartDate;

            if (saleEndDate.HasValue)
            {
                if (SaleStartDate.HasValue && saleEndDate.Value <= SaleStartDate.Value)
                    return Result.Failure("Sale end date must be after sale start date");

                SaleEndDate = saleEndDate;
            }

            if (isActive.HasValue)
                IsActive = isActive.Value;

            return Result.Success();
        }

    }
}
