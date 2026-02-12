using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Fulfillment.Application.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends ticket email with PDF attachment
        /// </summary>
        Task<(bool Success, string MessageId, string Response)> SendTicketEmailAsync(
            string recipientEmail,
            string recipientName,
            string orderNumber,
            string eventName,
            DateTime eventDate,
            string venueName,
            int ticketCount,
            byte[] pdfAttachment,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends generic email
        /// </summary>
        Task<(bool Success, string MessageId, string Response)> SendEmailAsync(
            string recipientEmail,
            string subject,
            string htmlBody,
            List<EmailAttachment>? attachments = null,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Email attachment model
    /// </summary>
    public sealed record EmailAttachment(
        string FileName,
        byte[] Content,
        string ContentType);

}
