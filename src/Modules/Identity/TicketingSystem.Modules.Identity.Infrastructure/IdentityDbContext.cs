using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Identity.Infrastructure
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

        // DbSets will be added here as we create entities
        // Example:
        // public DbSet<User> Users => Set<User>();
        // public DbSet<DeviceFingerprint> DeviceFingerprints => Set<DeviceFingerprint>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        }
    }
}
