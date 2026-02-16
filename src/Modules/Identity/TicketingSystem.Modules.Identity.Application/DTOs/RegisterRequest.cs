using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Identity.Application.DTOs
{
    public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Role);

}
