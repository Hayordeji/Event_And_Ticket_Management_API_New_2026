using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Domain.Entities;

namespace TicketingSystem.Modules.Identity.Application.Services
{
    /// <summary>
/// Service for generating JWT tokens
/// </summary>
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// Generate access token and refresh token
        /// </summary>
        string GenerateAccessToken(User user, string role);
    }
}
