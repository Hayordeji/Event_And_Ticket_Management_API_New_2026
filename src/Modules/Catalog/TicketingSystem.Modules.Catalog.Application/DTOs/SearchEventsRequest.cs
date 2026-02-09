using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Enums;

namespace TicketingSystem.Modules.Catalog.Application.DTOs
{
    public record SearchEventRequest(
    string? SearchTerm,
    Guid? HostId,
    EventStatus? Status,
    DateTime? StartDateFrom,
    DateTime? StartDateTo,
    string? City,
    string? Country,
    int PageNumber = 1,
    int PageSize = 20);
}
