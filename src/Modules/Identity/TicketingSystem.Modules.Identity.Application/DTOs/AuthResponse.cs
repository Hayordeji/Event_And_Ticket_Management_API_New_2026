using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Identity.Application.DTOs
{
    public record AuthResponse(Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
}
