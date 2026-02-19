using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TicketingSystem.SharedKernel.Outbox;

namespace TicketingSystem.SharedKernel.Persistence
{
     ///<summary>
/// Base DbContext with common configurations for all modules
/// </summary>
    public abstract class BaseDbContext : DbContext
    {
        private readonly string _schemaName;
        private readonly IMediator _mediator;


        protected BaseDbContext(DbContextOptions options, string schemaName, IMediator mediator) : base(options)
        {
            _schemaName = schemaName;
            _mediator = mediator;
        }

        /// <summary>
        /// Schema name for this module (e.g., "identity", "finance")
        /// </summary>
        protected string SchemaName => _schemaName;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default schema for all entities in this context
            modelBuilder.HasDefaultSchema(_schemaName);

            // Apply global query filters (soft delete)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Only apply to entities that inherit from Entity
                if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
                {
                    // Add soft delete filter
                    var method = typeof(BaseDbContext)
                        .GetMethod(nameof(SetGlobalQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(null, new object[] { modelBuilder });
                }
            }
        }

        private static void SetGlobalQueryFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : Entity
        {
            // Filter out soft-deleted entities by default
            modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Automatically set audit fields before saving
            UpdateAuditFields();

            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            //await DispatchDomainEventsAsync(cancellationToken);

              // Step 1: Collect all domain events from aggregates
            var domainEvents = GetDomainEvents();

            // Step 2: Persist domain events to the outbox (same transaction)
            await PersistDomainEventsToOutboxAsync(domainEvents, cancellationToken);

            // Step 3: Save everything atomically (entities + outbox messages)
            var result = await base.SaveChangesAsync(cancellationToken);

            // Step 4: Clear domain events from aggregates (they're now in the outbox)
            ClearDomainEvents();

           
            return result;

            //return await base.SaveChangesAsync(cancellationToken);
            //return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries<Entity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // CreatedAt is set in Entity constructor
                        // You can set CreatedBy here if you have access to current user
                        break;

                    case EntityState.Modified:
                        entry.Entity.MarkAsUpdated();
                        break;

                    case EntityState.Deleted:
                        // Convert hard delete to soft delete
                        entry.State = EntityState.Modified;
                        entry.Entity.MarkAsDeleted();
                        break;
                }
            }
        }

        private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
        {
            var aggregates = ChangeTracker
                .Entries<AggregateRoot>()
                .Where(x => x.Entity.DomainEvents.Any())
                .Select(x => x.Entity)
                .ToList();

            var domainEvents = aggregates
                .SelectMany(x => x.DomainEvents)
                .ToList();

            // Clear events before dispatching to prevent re-dispatching
            aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent);
            }
            
            await Task.CompletedTask;
        }

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        private List<IDomainEvent> GetDomainEvents()
        {
            return ChangeTracker
                .Entries<AggregateRoot>()
                .SelectMany(entry => entry.Entity.DomainEvents)
                .ToList();
        }

        private async Task PersistDomainEventsToOutboxAsync(
            List<IDomainEvent> domainEvents,
            CancellationToken cancellationToken)
        {
            foreach (var domainEvent in domainEvents)
            {
                var eventType = domainEvent.GetType().AssemblyQualifiedName
                    ?? throw new InvalidOperationException(
                        $"Cannot determine type name for event {domainEvent.GetType().Name}");

                var eventPayload = JsonSerializer.Serialize(
                    domainEvent,
                    domainEvent.GetType(),
                    new JsonSerializerOptions { WriteIndented = false });

                var outboxMessage = OutboxMessage.Create(
                    eventType: eventType,
                    eventPayload: eventPayload,
                    occurredAt: domainEvent.OccurredAt);

                await OutboxMessages.AddAsync(outboxMessage, cancellationToken);
            }
        }

        private void ClearDomainEvents()
        {
            var aggregates = ChangeTracker
                .Entries<AggregateRoot>()
                .Select(entry => entry.Entity)
                .ToList();

            foreach (var aggregate in aggregates)
                aggregate.ClearDomainEvents();
        }
    }
}
