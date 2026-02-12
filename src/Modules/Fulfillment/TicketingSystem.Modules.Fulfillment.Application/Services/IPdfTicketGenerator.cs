using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Domain.Entitites;

namespace TicketingSystem.Modules.Fulfillment.Application.Services
{
    public interface IPdfTicketGenerator
    {
        /// <summary>
        /// Generates a single PDF ticket
        /// </summary>
        byte[] GenerateTicketPdf(Ticket ticket);

        /// <summary>
        /// Generates a PDF with multiple tickets (batch)
        /// </summary>
        byte[] GenerateTicketsPdf(List<Ticket> tickets);
    }
}
