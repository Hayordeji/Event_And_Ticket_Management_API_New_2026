using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Identity.Application.DTOs
{
    public record RefreshTokenRequest(
        string RefreshToken
        );
   
}
