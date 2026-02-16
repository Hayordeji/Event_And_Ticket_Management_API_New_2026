using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Domain.Events;
using TicketingSystem.Modules.Identity.Domain.ValueObjects;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Domain.Entities
{
    /// <summary>
    /// Extends IdentityUser with domain-specific properties.
    /// Guid primary key — consistent with the rest of the system.
    /// </summary>
    public class User : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


        // Convenience — full name for display and ticket snapshots
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
