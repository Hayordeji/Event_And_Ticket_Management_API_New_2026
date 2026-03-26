using TicketingSystem.SharedKernel.Services;

namespace TicketingSystem.SharedKernel.DTOs
{
    /// <summary>
    /// Request to send generic email
    /// </summary>
    public record SendEmailRequest(
        string RecipientEmail,
        string Subject,
        string HtmlBody,
        List<EmailAttachment>? Attachments = null);
}