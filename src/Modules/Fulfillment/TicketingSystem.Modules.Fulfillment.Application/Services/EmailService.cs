using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Application.DTOs;

namespace TicketingSystem.Modules.Fulfillment.Application.Services
{
    /// <summary>
    /// Email service implementation
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _config;
        private readonly IResend _resend;

        public EmailService(ILogger<EmailService> logger, IConfiguration config, IResend resend)
        {
            _logger = logger;
            _config = config;
            _resend = resend;
        }

        public async Task<SendEmailResponse> SendTicketEmailAsync(
            string recipientEmail,
            string recipientName,
            string orderNumber,
            string eventName,
            DateTime eventDate,
            string venueName,
            int ticketCount,
            byte[] pdfAttachment,
            CancellationToken cancellationToken = default)
        {
            var response = new SendEmailResponse();

            try
            {
                _logger.LogInformation(
                    "Sending ticket email to {Email}. Order={OrderNumber}, Event={EventName}, TicketCount={TicketCount}",
                    recipientEmail, orderNumber, eventName, ticketCount);

                var subject = $"Your Tickets for {eventName}";
                var htmlBody = GenerateTicketEmailHtml(
                    recipientName,
                    orderNumber,
                    eventName,
                    eventDate,
                    venueName,
                    ticketCount);

                var attachments = new List<EmailAttachment>
            {
                new EmailAttachment(
                    FileName: $"Tickets-{orderNumber}.pdf", 
                    Content: pdfAttachment,
                    ContentType: "application/pdf") 
            };

                var result = await SendEmailAsync(
                    recipientEmail,
                    subject,
                    htmlBody,
                    attachments,
                    cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Ticket email sent successfully to {Email}. MessageId={MessageId}",
                        recipientEmail, result.MessageId);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send ticket email to {Email}. Response={Response}",
                        recipientEmail, result.Response);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error sending ticket email to {Email}",
                    recipientEmail);

                response.IsSuccess = false;
                response.MessageId = string.Empty;
                response.Response = $"Error: {ex.Message}";
                return (response);
            }
        }

        public async Task<SendEmailResponse> SendEmailAsync(
            string recipientEmail,
            string subject,
            string htmlBody,
            List<EmailAttachment>? attachments = null,
            CancellationToken cancellationToken = default)
        {
            var response = new SendEmailResponse();
            try
            {
                var apiKey = _config["Resend:ApiKey"];

                var email = new EmailMessage
                {
                    From = "noreply@yourdomain.com",
                    To = [recipientEmail],
                    Subject = subject,
                    HtmlBody = htmlBody
                };

                if (attachments != null)
                {

                    email.Attachments = attachments.Select(a => new Resend.EmailAttachment
                    {
                        Filename = a.FileName,
                        Content = Convert.ToBase64String(a.Content),
                        ContentType = a.ContentType
                    }).ToList();
                }

                var result = await _resend.EmailSendAsync(email, cancellationToken);

                response.IsSuccess = true;
                response.MessageId = result.Content.ToString();
                response.Response = "Email sent successfully";

                _logger.LogInformation(
                    "Email sent via Resend to {Email}. MessageId={MessageId}",
                    recipientEmail, response.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", recipientEmail);
                response.IsSuccess = false;
                response.MessageId = string.Empty;
                response.Response = $"Error: {ex.Message}";
            }

            return response;
        }

        private static string GenerateTicketEmailHtml(
            string recipientName,
            string orderNumber,
            string eventName,
            DateTime eventDate,
            string venueName,
            int ticketCount)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background: #4CAF50;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }}
        .content {{
            background: #f9f9f9;
            padding: 20px;
            border: 1px solid #ddd;
        }}
        .event-details {{
            background: white;
            padding: 15px;
            border-radius: 5px;
            margin: 15px 0;
        }}
        .button {{
            display: inline-block;
            background: #4CAF50;
            color: white;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 5px;
            margin: 15px 0;
        }}
        .footer {{
            text-align: center;
            padding: 20px;
            font-size: 12px;
            color: #666;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🎉 Your Tickets Are Ready!</h1>
    </div>
    
    <div class='content'>
        <p>Hi {recipientName},</p>
        
        <p>Thank you for your order! Your tickets for <strong>{eventName}</strong> are attached to this email.</p>
        
        <div class='event-details'>
            <h2>Event Details</h2>
            <p><strong>Event:</strong> {eventName}</p>
            <p><strong>Date:</strong> {eventDate:dddd, MMMM dd, yyyy}</p>
            <p><strong>Time:</strong> {eventDate:hh:mm tt}</p>
            <p><strong>Venue:</strong> {venueName}</p>
            <p><strong>Tickets:</strong> {ticketCount}</p>
            <p><strong>Order Number:</strong> {orderNumber}</p>
        </div>
        
        <p><strong>Important:</strong></p>
        <ul>
            <li>Please download and save your tickets</li>
            <li>Present the QR code at the entrance</li>
            <li>Arrive early to avoid queues</li>
            <li>Keep your tickets safe and do not share</li>
        </ul>
        
        <p>If you have any questions, please contact our support team.</p>
        
        <p>See you at the event! 🎊</p>
    </div>
    
    <div class='footer'>
        <p>This email was sent by Your Ticketing Platform</p>
        <p>Order #{orderNumber}</p>
    </div>
</body>
</html>";
        }
    }
}
