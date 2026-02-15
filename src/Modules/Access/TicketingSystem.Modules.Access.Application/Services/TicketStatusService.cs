using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Access.Application.Services;
using TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence;

namespace TicketingSystem.Modules.Access.Infrastructure.Services
{
    public class TicketStatusService : ITicketStatusService
    {
        private readonly FulfillmentDbContext _fulfillmentContext;

    public TicketStatusService(FulfillmentDbContext fulfillmentContext)
    {
        _fulfillmentContext = fulfillmentContext;
    }

    public async Task MarkAsUsedAsync(
        Guid ticketId,
        Guid scannedBy,
        string gateLocation,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _fulfillmentContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId && !t.IsDeleted, cancellationToken);

        if (ticket == null) return;

        ticket.MarkAsUsed(scannedBy, gateLocation);
        await _fulfillmentContext.SaveChangesAsync(cancellationToken);
    }
    }
}
