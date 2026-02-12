using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TicketingSystem.Modules.Sales.Domain.Entities;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Sales.Infrastructure.Persistence
{
    public class SalesDbContext : BaseDbContext
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();  

        public SalesDbContext(DbContextOptions<SalesDbContext> options, IMediator mediator) : base(options, "sales", mediator)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("sales");

            // Apply all configurations from current assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
