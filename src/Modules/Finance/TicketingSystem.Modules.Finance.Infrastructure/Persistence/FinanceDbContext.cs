using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Entities;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Finance.Infrastructure.Persistence
{
    public class FinanceDbContext : BaseDbContext
    {
        public FinanceDbContext(DbContextOptions<FinanceDbContext> options, IMediator mediator)
        : base(options, "finance", mediator)
        {
        }

        public DbSet<LedgerAccount> LedgerAccounts { get; set; }
        public DbSet<LedgerTransaction> LedgerTransactions => Set<LedgerTransaction>();
        public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceDbContext).Assembly);
        }
    }
}

