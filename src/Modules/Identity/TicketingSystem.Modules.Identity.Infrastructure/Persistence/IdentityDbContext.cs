using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Domain.Entities;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Identity.Infrastructure.Persistence
{
    ///<summary>
/// Database context for Identity module
/// Schema: "identity"
/// </summary>
    public class IdentityDbContext : BaseDbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options, "identity")
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        }
    }
}
