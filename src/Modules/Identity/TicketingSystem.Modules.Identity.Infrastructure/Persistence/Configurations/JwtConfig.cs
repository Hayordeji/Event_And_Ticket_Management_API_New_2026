using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class JwtConfig
    {
        public const string SectionName = "Jwt";

        public required string Secret { get; init; }
        public required string Issuer { get; init; }
        public required string Audience { get; init; }
        public int ExpiryMinutes { get; init; } = 60; // Default 60 mins
    }
}
