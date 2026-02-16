using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Domain.Entities;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Identity.Infrastructure.Persistence
{
    ///<summary>
/// Database context for Identity module
/// Schema: "identity"
/// </summary>
    public class IdentityAppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public IdentityAppDbContext(DbContextOptions<IdentityAppDbContext> options)
        : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("identity");

            // Apply all entity configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityAppDbContext).Assembly);
        }
    }
}
