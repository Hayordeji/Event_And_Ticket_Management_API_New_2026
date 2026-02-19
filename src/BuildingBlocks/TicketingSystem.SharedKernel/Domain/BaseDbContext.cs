using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TicketingSystem.SharedKernel.Outbox;

namespace TicketingSystem.SharedKernel.Domain
{
    public class BaseDbContext : DbContext
    {
       
    }
}
