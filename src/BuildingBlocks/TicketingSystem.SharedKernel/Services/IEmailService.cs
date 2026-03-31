using TicketingSystem.SharedKernel.DTOs;

namespace TicketingSystem.SharedKernel.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends ticket email with PDF attachment
        /// </summary>
        Task<SendEmailResponse> SendTicketEmailAsync(
            SendTicketEmailRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends generic email
        /// </summary>
        Task<SendEmailResponse> SendEmailAsync(
            SendEmailRequest request,
            CancellationToken cancellationToken = default);

        Task<SendEmailResponse> SendWelcomeEmailAsync(
        string recipientEmail, string recipientName, CancellationToken ct = default);

        Task<SendEmailResponse> SendEmailVerificationAsync(
            string recipientEmail, string recipientName,
            string verificationLink, CancellationToken ct = default);

        Task<SendEmailResponse> SendPasswordResetAsync(
            string recipientEmail, string recipientName,
            string resetLink, CancellationToken ct = default);

        Task<SendEmailResponse> SendAccountLockedAsync(
            string recipientEmail, string recipientName,
            DateTime unlocksAt, CancellationToken ct = default);

        Task<SendEmailResponse> SendOrderConfirmationEmailAsync(
            string recipientEmail, string recipientName,
            string orderNumber, string eventName,
            DateTime eventDate, string venueName,
            decimal totalAmount, int ticketCount,
            CancellationToken ct = default);

        Task<SendEmailResponse> SendOrderCancelledEmailAsync(
            string recipientEmail, string recipientName,
            string orderNumber, string eventName,
            string cancellationReason,
            CancellationToken ct = default);

        Task<SendEmailResponse> SendOrderExpiredEmailAsync(
            string recipientEmail, string recipientName,
            string orderNumber, string eventName,
            CancellationToken ct = default);
        Task<SendEmailResponse> SendOrderCreatedEmailAsync(string recipientEmail,
            string recipientName,
            string orderNumber,
            string eventName,
            DateTime createdAt,
            CancellationToken ct = default);

        Task<SendEmailResponse> SendOrderRefundedEmailAsync(
            string recipientEmail,
            string recipientName,
            string orderNumber,
            string eventName,
            decimal refundAmount,
            string currency,
            CancellationToken ct = default);

        /// <summary>
        /// Sends event cancellation notification to attendees
        /// </summary>
        Task<SendEmailResponse> SendEventCancelledEmailAsync(
            string recipientEmail,
            string recipientName,
            string eventName,
            string reason,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends event update notification to attendees
        /// </summary>
        Task<SendEmailResponse> SendEventUpdatedEmailAsync(
            string recipientEmail,
            string recipientName,
            string eventName,
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
