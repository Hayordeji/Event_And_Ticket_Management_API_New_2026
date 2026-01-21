using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Identity.Application.DTOs
{
    public record LoginRequest(
    string Email,
    string Password);
    
}
