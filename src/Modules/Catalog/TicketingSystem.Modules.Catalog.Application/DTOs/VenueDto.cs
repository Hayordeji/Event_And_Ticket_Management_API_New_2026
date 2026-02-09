using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Catalog.Application.DTOs
{
    public record VenueDto(
    string Name,
    string Address,
    string City,
    string State,
    string Country,
    string? PostalCode,
    decimal? Latitude,
    decimal? Longitude);
}
