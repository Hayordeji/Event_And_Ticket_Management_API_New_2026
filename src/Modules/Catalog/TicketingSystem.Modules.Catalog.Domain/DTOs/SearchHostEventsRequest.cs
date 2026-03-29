using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Enums;

namespace TicketingSystem.Modules.Catalog.Domain.DTOs
{
    public class SearchHostEventsRequest
    {
        public string? searchTerm { get; set; }
        public EventStatus? status { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }


    }
}
