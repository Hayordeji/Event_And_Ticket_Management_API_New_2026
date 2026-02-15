using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Entities;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Catalog.Infrastructure.Persistence
{
    public class CatalogDbContext : BaseDbContext
    {
        private readonly DomainEventDispatcher _dispatcher;
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options, IMediator mediator) : base(options, "catalog", mediator)
        {
        }


        public DbSet<Event> Events => Set<Event>();
        public DbSet<EventSnapshot> EventSnapshots => Set<EventSnapshot>();
        public DbSet<TicketType> TicketTypes => Set<TicketType>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default schema for Catalog module
            modelBuilder.HasDefaultSchema("catalog");

            // Apply all configurations in this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        }
    }
}
