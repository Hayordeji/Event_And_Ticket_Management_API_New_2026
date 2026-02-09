using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Services
{
    public  interface IEventValidationService
    {
        /// <summary>
        /// Validates event existence and retrieves complete event data including ticket types
        /// </summary>
        Task<EventValidationResult> ValidateEventAsync(Guid eventId, CancellationToken cancellationToken = default);

        Task<Result<EventValidationResponse>> ValidateEventAndTicketTypesAsync(
        Guid eventId,
        List<Guid> ticketTypeIds,
        CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates order items (capacity, pricing, availability) before order creation
        /// </summary>
        Task<(bool IsValid, List<string> Errors)> ValidateOrderItemsAsync(
        Guid eventId,
        List<OrderItemValidationDto> items,
        CancellationToken cancellationToken = default); 

        /// <summary>
        /// Gets current sold count for a ticket type (for capacity checks)
        /// </summary>
        Task<int> GetTicketTypeSoldCountAsync(Guid ticketTypeId, CancellationToken cancellationToken = default);
    }

    public record EventValidationResponse(
    bool IsValid,
    string? ErrorMessage = null
);
}
