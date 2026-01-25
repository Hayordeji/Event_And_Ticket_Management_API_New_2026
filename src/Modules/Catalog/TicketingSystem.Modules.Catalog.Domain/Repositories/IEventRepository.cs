using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Application.DTOs;
using TicketingSystem.Modules.Catalog.Domain.Entities;
using TicketingSystem.Modules.Catalog.Domain.Enums;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Domain.Repositories
{
    public interface IEventRepository : IRepository<Event>
    {
        /// <summary>
        /// Get event with all ticket types loaded
        /// </summary>
        Task<Event?> GetByIdWithTicketTypesAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get event with all snapshots loaded
        /// </summary>
        Task<Event?> GetByIdWithSnapshotsAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get event with both ticket types and snapshots
        /// </summary>
        Task<Event?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get all events by a specific host
        /// </summary>
        Task<List<Event>> GetEventsByHostIdAsync(Guid hostId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Get events by status
        /// </summary>
        Task<List<Event>> GetEventsByStatusAsync(EventStatus status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Search events with filters
        /// </summary>
        Task<(List<Event> Events, int TotalCount)> SearchEventsAsync(SearchEventsRequest request,CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a specific ticket type by ID (through the event aggregate)
        /// </summary>
        Task<TicketType?> GetTicketTypeByIdAsync(Guid ticketTypeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all ticket types for an event
        /// </summary>
        Task<List<TicketType>> GetTicketTypesByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get only active/available ticket types for an event
        /// </summary>
        Task<List<TicketType>> GetAvailableTicketTypesByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
