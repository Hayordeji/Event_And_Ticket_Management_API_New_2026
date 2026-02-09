using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Sales.Application.DTOs
{
    public class EventValidationResponse
    {

    }

    /// <summary>
    /// DTO containing comprehensive event validation data including capacity, pricing, and snapshot information
    /// </summary>
    public sealed record EventValidationDto
    {
        public Guid EventId { get; init; }
        public string EventName { get; init; } = string.Empty;
        public string EventDescription { get; init; } = string.Empty;
        public DateTime EventStartDate { get; init; }
        public DateTime EventEndDate { get; init; }
        public string VenueName { get; init; } = string.Empty;
        public string VenueAddress { get; init; } = string.Empty;
        public string VenueCity { get; init; } = string.Empty;
        public bool IsCancelled { get; init; }
        public List<TicketTypeValidationDto> TicketTypes { get; init; } = new();
    }

    /// <summary>
    /// DTO containing ticket type validation data including availability and pricing
    /// </summary>
    public sealed record TicketTypeValidationDto
    {
        public Guid TicketTypeId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public string Currency { get; init; } = string.Empty;
        public int TotalCapacity { get; init; }
        public int SoldCount { get; init; }
        public int AvailableCount => TotalCapacity - SoldCount;
        public bool IsSoldOut => AvailableCount <= 0;
        public bool IsActive { get; init; }
        public DateTime? SaleStartDate { get; init; }
        public DateTime? SaleEndDate { get; init; }
        public bool IsSaleActive
        {
            get
            {
                var now = DateTime.UtcNow;
                var saleStarted = !SaleStartDate.HasValue || SaleStartDate.Value <= now;
                var saleNotEnded = !SaleEndDate.HasValue || SaleEndDate.Value >= now;
                return IsActive && saleStarted && saleNotEnded;
            }
        }
    }

    /// <summary>
    /// Result of event validation containing validation errors if any
    /// </summary>
    public sealed record EventValidationResult
    {
        public bool IsValid { get; init; }
        public EventValidationDto? EventData { get; init; }
        public List<string> Errors { get; init; } = new();

        public static EventValidationResult Success(EventValidationDto data) => new()
        {
            IsValid = true,
            EventData = data
        };

        public static EventValidationResult Failure(params string[] errors) => new()
        {
            IsValid = false,
            Errors = errors.ToList()
        };
    }

    /// <summary>
    /// DTO for order item validation containing ticket type data and requested quantity
    /// </summary>
    public sealed record OrderItemValidationDto
    {
        public Guid TicketTypeId { get; init; }
        public int RequestedQuantity { get; init; }
        public decimal SubmittedUnitPrice { get; init; }
    }
}
