using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TicketingSystem.Modules.Catalog.Application.DTOs;
using TicketingSystem.Modules.Catalog.Domain.Entities;
using TicketingSystem.Modules.Catalog.Domain.Enums;
using TicketingSystem.Modules.Catalog.Domain.Repositories;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Catalog.Infrastructure.Persistence.Repositories
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        private readonly CatalogDbContext _context;

        public EventRepository(CatalogDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Event?> GetByIdWithTicketTypesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Events
                .AsNoTracking()
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<Event?> GetByIdWithSnapshotsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Events
                .Include(e => e.Snapshots.OrderByDescending(s => s.SnapshotVersion))
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<Event?> GetByIdWithAllRelatedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Events
                .Include(e => e.TicketTypes)
                .Include(e => e.Snapshots.OrderByDescending(s => s.SnapshotVersion))
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Event>> GetByHostIdAsync(Guid hostId, CancellationToken cancellationToken = default)
        {
            return await _context.Events
                .Where(e => e.HostId == hostId)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Event>> GetPublishedEventsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Events
                .Where(e => e.PublishedAt >= DateTime.Parse("1-1-2026") && e.PublishedAt <= DateTime.UtcNow);

            if (fromDate.HasValue)
            {
                query = query.Where(e => e.StartDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(e => e.StartDate <= toDate.Value);
            }

            return await query
                .OrderBy(e => e.StartDate)
                .ToListAsync(cancellationToken);
        }


        public async Task<int> GetTotalEventsCountAsync(
            string? searchTerm,
            string? category,
            string? city,
            DateTime? fromDate,
            DateTime? toDate,
            bool onlyPublished = true,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Events.AsQueryable();

          
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(e =>
                    e.Name.ToLower().Contains(term) ||
                    e.Description.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                var cityLower = city.ToLower();
                query = query.Where(e => e.Venue.City.ToLower() == cityLower);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(e => e.StartDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(e => e.StartDate <= toDate.Value);
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<TicketType?> GetTicketTypeByIdAsync(Guid ticketTypeId, CancellationToken cancellationToken = default)
        {
            return await _context.TicketTypes
                .FirstOrDefaultAsync(t => t.Id == ticketTypeId, cancellationToken);
        }

        public async Task<List<TicketType>> GetTicketTypesByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.TicketTypes
                .Where(t => t.EventId == eventId)
                .OrderByDescending(t => t.CreatedAt)
                .ThenBy(t => t.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<EventSnapshot?> GetSnapshotByVersionAsync(Guid eventId, int version, CancellationToken cancellationToken = default)
        {
            return await _context.EventSnapshots
                .FirstOrDefaultAsync(s => s.EventId == eventId && s.SnapshotVersion == version, cancellationToken);
        }

        public async Task<EventSnapshot?> GetLatestSnapshotAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.EventSnapshots
                .Where(s => s.EventId == eventId)
                .OrderByDescending(s => s.SnapshotVersion)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<int> GetNextSnapshotVersionAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            var latestSnapshot = await GetLatestSnapshotAsync(eventId, cancellationToken);
            return latestSnapshot == null ? 1 : latestSnapshot.SnapshotVersion + 1;
        }

        public async Task AddSnapshotAsync(EventSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            await _context.EventSnapshots.AddAsync(snapshot, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Events.AnyAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<Event?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Events
                .Include(e => e.TicketTypes)
                .Include(e => e.Snapshots.OrderByDescending(s => s.SnapshotVersion))
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<List<Event>> GetEventsByHostIdAsync(Guid hostId, CancellationToken cancellationToken = default)
        {
            return await _context.Events
                            .Where(e => e.HostId == hostId)
                            .OrderByDescending(e => e.StartDate)
                            .ToListAsync(cancellationToken);
        }

        public async Task<List<Event>> GetEventsByStatusAsync(EventStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.Events
                .Where(e => e.Status == status)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<Event> Events, int TotalCount)> SearchEventsAsync(SearchEventsRequest request, CancellationToken cancellationToken = default)
        {
            var query = _context.Events.AsQueryable();

            // Search in title and description
            if (!string.IsNullOrWhiteSpace(request.searchTerm))
            {
                var term = request.searchTerm.ToLower();
                query = query.Where(e =>
                    e.Name.ToLower().Contains(term) ||
                    e.Description.ToLower().Contains(term));
            }

            // Filter by city (using Venue value object)
            if (!string.IsNullOrWhiteSpace(request.City))
            {
                var cityLower = request.City.ToLower();
                query = query.Where(e => e.Venue.City.ToLower() == cityLower);
            }

            // Filter by date range
            if (request.StartDateFrom.HasValue)
            {
                query = query.Where(e => e.StartDate >= request.StartDateFrom.Value);
            }

            if (request.StartDateTo.HasValue)
            {
                query = query.Where(e => e.StartDate <= request.StartDateTo.Value);
            }

            // Total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Pagination
            var skip = (request.PageNumber - 1) * request.PageSize;

            var results = await query
                .OrderBy(e => e.StartDate)
                .Skip(skip.Value)
                .Take(request.PageSize.Value)
                .ToListAsync(cancellationToken);

            return (results, totalCount);
        }

        public async Task<List<TicketType>> GetAvailableTicketTypesByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _context.TicketTypes
               .Where(t => EF.Property<Guid>(t, "EventId") == eventId
                           && t.IsActive
                           && (t.SaleStartDate == null || t.SaleStartDate <= now)
                           && (t.SaleEndDate == null || t.SaleEndDate >= now)
                           && (t.TotalCapacity - t.SoldCount - t.ReservedCount) > 0)
               .OrderByDescending(t => t.CreatedAt)
               .ThenBy(t => t.Name)
               .ToListAsync(cancellationToken);
        }
    }
}
