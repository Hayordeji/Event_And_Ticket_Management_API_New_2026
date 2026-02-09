using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Catalog.Application.DTOs
{
    public record CreateEventRequest(
    string Name,
    string Description,
    VenueDto Venue,
    DateTime StartDate,
    DateTime? EndDate,
    string? ImageUrl);
}
