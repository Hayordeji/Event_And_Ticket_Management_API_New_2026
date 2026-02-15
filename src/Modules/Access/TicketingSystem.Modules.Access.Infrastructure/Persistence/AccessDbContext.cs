using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Access.Domain.Entities;
using TicketingSystem.Modules.Access.Infrastructure.Persistence.Configuration;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Access.Infrastructure.Persistence
{
    public class AccessDbContext : BaseDbContext
    {

        public AccessDbContext(DbContextOptions<AccessDbContext> options, IMediator mediator) : base(options, "access", mediator) { }

        public DbSet<ScanLog> ScanLogs => Set<ScanLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("access");
            modelBuilder.ApplyConfiguration(new ScanLogConfiguration());
        }
    }
}
