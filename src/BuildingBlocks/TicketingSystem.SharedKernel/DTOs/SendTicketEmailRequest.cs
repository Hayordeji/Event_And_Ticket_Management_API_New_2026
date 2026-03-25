using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.DTOs
{
    /// <summary>
    /// Request to send ticket email with PDF attachment
    /// </summary>
    public record SendTicketEmailRequest(
        string RecipientEmail,
        string RecipientName,
        string OrderNumber,
        string EventName,
        DateTime EventDate,
        string VenueName,
        int TicketCount,
        byte[] PdfAttachment);
}