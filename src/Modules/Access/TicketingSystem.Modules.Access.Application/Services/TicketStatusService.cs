using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Access.Application.Services;
using TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Access.Infrastructure.Services
{
    public class TicketStatusService : ITicketStatusService
    {
        private readonly FulfillmentDbContext _fulfillmentContext;

    public TicketStatusService(FulfillmentDbContext fulfillmentContext)
    {
        _fulfillmentContext = fulfillmentContext;
    }

    public async Task<Result> MarkAsUsedAsync(
        Guid ticketId,
        Guid scannedBy,
        string gateLocation,
        CancellationToken cancellationToken = default)
        {

            try
            {
                var ticket = await _fulfillmentContext.Tickets
                .FirstOrDefaultAsync(t => t.Id == ticketId && !t.IsDeleted, cancellationToken);

                if (ticket == null) return Result.Failure("Ticket was not found in the database");

                ticket.MarkAsUsed(scannedBy, gateLocation);
                await _fulfillmentContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);

            }
            
        }
    }
}
