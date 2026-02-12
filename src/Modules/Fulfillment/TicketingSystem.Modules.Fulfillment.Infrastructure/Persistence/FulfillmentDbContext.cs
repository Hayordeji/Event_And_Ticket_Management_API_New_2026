using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Domain.Entitites;
using TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence.Configurations;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence
{
    public class FulfillmentDbContext : BaseDbContext
    {
        public FulfillmentDbContext(DbContextOptions<FulfillmentDbContext> options, IMediator mediator)
        : base(options,"fufillment", mediator)
        {
        }

        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<TicketDelivery> TicketDeliveries => Set<TicketDelivery>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default schema
            modelBuilder.HasDefaultSchema("fulfillment");

            // Apply configurations
            modelBuilder.ApplyConfiguration(new TicketConfiguration());
            modelBuilder.ApplyConfiguration(new TicketDeliveryConfiguration());
        }
    }
}
